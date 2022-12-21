
using ServiceIntegrationLibrary.Models;

namespace TransactionsApi.Data.Repositories
{
    public interface IPaymentsRepository
    {
        public Task<PaymentDetails> Get(string transactionId);
        public Task<IEnumerable<PaymentDetails>> Get(long merchantId, DateTime from, DateTime? to = null);
        public Task Post(PaymentDetails payment);
        public Task UpdateIfExistsElseInsert(PaymentDetails payment);
    }
}
