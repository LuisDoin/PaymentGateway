using MassTransit;
using Microsoft.Extensions.Options;
using Model;
using Model.ModelValidationServices;
using Model.Utils;
using PaymentProcessor.Config;
using PaymentProcessor.Mappers.Interfaces;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace PaymentProcessor.Consumers
{
    public class PaymentConsumer : IConsumer<PaymentDetails>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPaymentValidationService _paymentValidationService;
        private readonly ICKOMapper _ckoMapper;
        private readonly ILogger<PaymentConsumer> _logger;
        private readonly CKOBankSettings _cKOBankSettings;
        private readonly RabbitMQSettings _rabbitMQSettings;

        public PaymentConsumer(IHttpClientFactory httpClientFactory,
            IPaymentValidationService paymentValidationService,
            ICKOMapper ckoMapper,
            ILogger<PaymentConsumer> logger,
            IOptions<CKOBankSettings> CKOBankOptions,
            IOptions<RabbitMQSettings> rabbitMQOptions)
        {
            _httpClientFactory = httpClientFactory;
            _paymentValidationService = paymentValidationService;
            _ckoMapper = ckoMapper;
            _logger = logger;
            _cKOBankSettings = CKOBankOptions.Value;
            _rabbitMQSettings = rabbitMQOptions.Value;
        }

        public async Task Consume(ConsumeContext<PaymentDetails> context)
        {
            try
            {
                _logger.LogInformation($"Processing payment {context.Message.PaymentId}");

                var message = context.Message;

                _paymentValidationService.ValidatePayment(message);

                var ckoPaymentInfoDTO = _ckoMapper.ToDto(message);

                var httpClient = _httpClientFactory.CreateClient();
                var paymentJson = new StringContent(JsonSerializer.Serialize(ckoPaymentInfoDTO), Encoding.UTF8, Application.Json); 
                using var httpResponseMessage = await httpClient.PostAsync(_cKOBankSettings.Uri, paymentJson);

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
