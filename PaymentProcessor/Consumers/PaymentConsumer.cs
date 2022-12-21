﻿using MassTransit;
using Microsoft.Extensions.Options;

using PaymentProcessor.Config;
using PaymentProcessor.Mappers.Interfaces;
using ServiceIntegrationLibrary.Models;
using ServiceIntegrationLibrary.ModelValidationServices;
using ServiceIntegrationLibrary.Utils;
using ServiceIntegrationLibrary.Utils.Interfaces;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace PaymentProcessor.Consumers
{
    public class PaymentConsumer : IConsumer<IncomingPayment>
    {
        private readonly IHttpClientProvider _httpClientProvider;
        private readonly IPaymentValidationService _paymentValidationService;
        private readonly ICKOMapper _ckoMapper;
        private readonly ILogger<PaymentConsumer> _logger;
        private readonly CKOBankSettings _cKOBankSettings;
        private readonly RabbitMQSettings _rabbitMQSettings;

        public PaymentConsumer(IHttpClientProvider httpClientProvider,
            IPaymentValidationService paymentValidationService,
            ICKOMapper ckoMapper,
            ILogger<PaymentConsumer> logger,
            IOptions<CKOBankSettings> CKOBankOptions,
            IOptions<RabbitMQSettings> rabbitMQOptions)
        {
            _httpClientProvider = httpClientProvider;
            _paymentValidationService = paymentValidationService;
            _ckoMapper = ckoMapper;
            _logger = logger;
            _cKOBankSettings = CKOBankOptions.Value;
            _rabbitMQSettings = rabbitMQOptions.Value;
        }

        public async Task Consume(ConsumeContext<IncomingPayment> context)
        {
            try
            {
                _logger.LogInformation($"Processing payment {context.Message.PaymentId}");

                var message = context.Message;

                _paymentValidationService.ValidatePayment(message);

                var ckoPaymentInfoDTO = _ckoMapper.ToDto(message);
                var paymentJson = JsonSerializer.Serialize(ckoPaymentInfoDTO);

                _logger.LogInformation($"Sending payment {context.Message.PaymentId} to CKOBranSimulator");

                using var httpResponseMessage = await _httpClientProvider.PostAsync(_cKOBankSettings.Uri, paymentJson);

                httpResponseMessage.EnsureSuccessStatusCode();

                message.Status = PaymentStatus.Successful;

                var sendEndpoint = await context.GetSendEndpoint(new Uri($"queue:{_rabbitMQSettings.CompletedTransactionsQueue}"));
                await sendEndpoint.Send(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while processing payment {context.Message.PaymentId}.");
                throw;
            }
        }
    }
}
