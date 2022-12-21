using ServiceIntegrationLibrary.Models;
using TransactionsApi.Data.Repositories;

namespace TransactionsApi.Services
{
    public class PaymentServices : IPaymentServices
    {
        private readonly ILogger<PaymentServices> _logger;
        private readonly IPaymentsRepository _paymentsRepository;

        public PaymentServices(ILogger<PaymentServices> logger, IPaymentsRepository paymentRepository)
        {
            _logger = logger;
            _paymentsRepository = paymentRepository;
        }

        public async Task ProcessCompletedTransaction(PaymentDetails paymentDetails)
        {
            try
            {
                _logger.LogInformation($"Saving payment {paymentDetails.PaymentId} to db");

                await _paymentsRepository.UpdateIfExistsElseInsert(paymentDetails);
            }
            catch (Exception e)
            {

                throw;
            }
        }
    }
}
