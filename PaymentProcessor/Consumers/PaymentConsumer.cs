using MassTransit;
using Model;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace PaymentProcessor.Consumers
{
    public class PaymentConsumer : IConsumer<PaymentDetails>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<PaymentConsumer> _logger;

        public PaymentConsumer(IHttpClientFactory httpClientFactory, IPublishEndpoint publishEndpoint, ILogger<PaymentConsumer> logger)
        {
            _httpClientFactory = httpClientFactory;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentDetails> context)
        {
            try
            {
                //Add validation.

                var message = context.Message;
                var paymentId = message.PaymentId;

                _logger.LogInformation("Processing payment:" + paymentId);

                var httpClient = _httpClientFactory.CreateClient();
                var paymentJson = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, Application.Json); 

                using var httpResponseMessage = await httpClient.PostAsync("CKOBankApi/Payments", paymentJson);

                httpResponseMessage.EnsureSuccessStatusCode();

                await _publishEndpoint.Publish(context.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while processing message " + context.Message + " - Error: " + ex);
                throw;
            }
        }
    }
}
