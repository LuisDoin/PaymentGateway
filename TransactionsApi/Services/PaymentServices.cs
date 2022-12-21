using Microsoft.Extensions.Options;
using ServiceIntegrationLibrary.Models;
using ServiceIntegrationLibrary.Utils;
using ServiceIntegrationLibrary.Utils.Interfaces;
using System.Text;
using System.Text.Json;
using TransactionsApi.Config;
using TransactionsApi.Data.Repositories;
using static System.Net.Mime.MediaTypeNames;

namespace TransactionsApi.Services
{
    public class PaymentServices : IPaymentServices
    {
        private readonly IHttpClientProvider _httpClientProvider;
        private readonly ILogger<PaymentServices> _logger;
        private readonly IPaymentsRepository _paymentRepository;
        private readonly PaymentGatewaySettings _paymentGatewaySettings;

        public PaymentServices(IHttpClientProvider httpClientProvider, ILogger<PaymentServices> logger, IPaymentsRepository paymentRepository, IOptions<PaymentGatewaySettings> paymentGatewayOptions)
        {
            _httpClientProvider = httpClientProvider;
            _logger = logger;
            _paymentRepository = paymentRepository;
            _paymentGatewaySettings = paymentGatewayOptions.Value;
        }

        public async Task ProcessCompletedTransaction(IncomingPayment paymentDetails)
        {
            _logger.LogInformation($"Saving payment {paymentDetails.PaymentId} to db");

            await _paymentRepository.Post(new ProcessedPayment(paymentDetails));

            _logger.LogInformation($"Sending response to PaymentGateway for payment {paymentDetails.PaymentId}");

            var paymentJson = JsonSerializer.Serialize(paymentDetails);
            using var httpResponseMessage = await _httpClientProvider.PostAsync(_paymentGatewaySettings.Uri, paymentJson);

            httpResponseMessage.EnsureSuccessStatusCode();
        }
    }
}
