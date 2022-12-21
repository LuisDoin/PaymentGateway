
using ServiceIntegrationLibrary.Models;

namespace TransactionsApi.Data.Repositories
{
    public interface IPaymentsRepository
    {
        public Task<ProcessedPayment> Get(string transactionId);
        public Task<IEnumerable<ProcessedPayment>> Get(long merchantId, DateTime from, DateTime? to = null);
        public Task Post(ProcessedPayment paymentDetails);
    }
}
