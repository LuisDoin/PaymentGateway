using Dapper;
using ServiceIntegrationLibrary.Models;
using System.Data;
using System.Text.Json;
using System.Transactions;
using TransactionsApi.Context;
using TransactionsApi.Data.Utils;
using TransactionsApi.Models;

namespace TransactionsApi.Data.Repositories
{
    public class PaymentsRepository : IPaymentsRepository
    {
        private IDbConnection _dbConnection;

        public PaymentsRepository(DapperContext dapperContext)
        {
            _dbConnection = dapperContext.CreateConnection();
        }

        public async Task<Payment> Get(Guid transactionId)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<Payment>(SqlQueries.GetTransaction, new { transactionId });
        }

        public async Task Post(Payment payment)
        {
            await _dbConnection.ExecuteAsync(SqlQueries.PostTransaction, new { payment.PaymentId, payment.MerchantId, payment.CreditCardNumber, payment.ExpirationDate, payment.Cvv, payment.Currency, payment.Amount, payment.CreationDate, Status = payment.Status.ToString() });
        }
    }
}
