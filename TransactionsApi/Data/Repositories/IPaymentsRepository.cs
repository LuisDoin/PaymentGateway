using TransactionsApi.Models;

namespace TransactionsApi.Data.Repositories
{
    public interface IPaymentsRepository
    {
        public Task<Payment> Get(Guid transactionId);

        public Task Post(Payment paymentDetails);
    }
}
