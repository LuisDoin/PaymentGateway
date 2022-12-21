
using ServiceIntegrationLibrary.Models;

namespace TransactionsApi.Data.Repositories
{
    public interface IPaymentsRepository
    {
        public Task<ProcessedPayment> Get(string transactionId);

        public Task Post(ProcessedPayment paymentDetails);
    }
}
