using MassTransit;
using Model;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using PaymentProcessor.Config;
using Microsoft.Extensions.Options;
using Model.ModelValidationServices;
using PaymentProcessor.Mappers;
using PaymentProcessor.Mappers.Interfaces;
using Model.Utils;

namespace PaymentProcessor.Consumers
{
    public class PaymentConsumer : IConsumer<PaymentDetails>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<PaymentConsumer> _logger;
        private readonly CKOBankSettings _cKOBankSettings;
        private readonly IPaymentValidationService _paymentValidationService;
        private readonly ICKOMapper _ckoMapper;

        public PaymentConsumer(IHttpClientFactory httpClientFactory,
            IPublishEndpoint publishEndpoint,
            ILogger<PaymentConsumer> logger,
            IOptions<CKOBankSettings> options,
            IPaymentValidationService paymentValidationService,
            ICKOMapper ckoMapper)
        {
            _httpClientFactory = httpClientFactory;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
            _cKOBankSettings = options.Value;
            _paymentValidationService = paymentValidationService;
            _ckoMapper = ckoMapper;
        }

        public async Task Consume(ConsumeContext<PaymentDetails> context)
        {
            try
            {
                _logger.LogInformation("Processing payment {PaymentId}.", context.Message.PaymentId);

                var message = context.Message;

                _paymentValidationService.ValidatePayment(message);

                var ckoPaymentInfoDTO = _ckoMapper.ToDto(message);

                var httpClient = _httpClientFactory.CreateClient();
                var paymentJson = new StringContent(JsonSerializer.Serialize(ckoPaymentInfoDTO), Encoding.UTF8, Application.Json); 
                using var httpResponseMessage = await httpClient.PostAsync(_cKOBankSettings.Uri, paymentJson);

                httpResponseMessage.EnsureSuccessStatusCode();

                message.Status = PaymentStatus.Successful;

                await _publishEndpoint.Publish(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing payment {PaymentId}." , context.Message.PaymentId);
                throw;
            }
        }
    }
}
