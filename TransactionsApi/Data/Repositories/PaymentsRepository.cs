using Dapper;
using ServiceIntegrationLibrary.Models;
using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Transactions;
using TransactionsApi.Context;
using TransactionsApi.Data.Utils;

namespace TransactionsApi.Data.Repositories
{
    public class PaymentsRepository : IPaymentsRepository
    {
        private IDbConnection _dbConnection;

        public PaymentsRepository(DapperContext dapperContext)
        {
            _dbConnection = dapperContext.CreateConnection();
        }

        public async Task<Payment> Get(string paymentId)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<Payment>(SqlQueries.GetTransaction, new { paymentId });
        }

        public async Task Post(Payment payment)
        {
            payment.CreditCard = MaskCreditCardNumber(payment.CreditCard);
            await _dbConnection.ExecuteAsync(SqlQueries.PostTransaction, new { payment.PaymentId, payment.MerchantId, payment.CreditCard, payment.ExpirationDate, payment.Cvv, payment.Currency, payment.Amount, payment.CreationDate, Status = payment.Status.ToString() });
        }

        private string MaskCreditCardNumber(string creditCardNumber)
        {
            var lastDigits = creditCardNumber.Substring(creditCardNumber.Length - 4, 4);
            var requiredMask = new String('X', creditCardNumber.Length - lastDigits.Length);
            var maskedString = string.Concat(requiredMask, lastDigits);
            var maskedCardNumberWithSpaces = Regex.Replace(maskedString, ".{4}", "$0 ");
            return maskedCardNumberWithSpaces;
        }
    }
}
