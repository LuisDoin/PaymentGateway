
using ServiceIntegrationLibrary.Models;

namespace TransactionsApi.Data.Repositories
{
    public interface IPaymentsRepository
    {
        public Task<Payment> Get(string transactionId);

        public Task Post(Payment paymentDetails);
    }
}
