using Microsoft.Extensions.Options;
using ServiceIntegrationLibrary.Models;
using ServiceIntegrationLibrary.Utils.Interfaces;
using System.Text.Json;
using TransactionsApi.Config;
using TransactionsApi.Data.Repositories;
using TransactionsApi.Models;

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

        public async Task ProcessCompletedTransaction(PaymentDetails paymentDetails)
        {
            await _paymentRepository.Post(new Payment(paymentDetails));

            var paymentJson = JsonSerializer.Serialize(paymentDetails);
            using var httpResponseMessage = await _httpClientProvider.PostAsync(_paymentGatewaySettings.Uri, paymentJson);

            httpResponseMessage.EnsureSuccessStatusCode();
        }
    }
}
