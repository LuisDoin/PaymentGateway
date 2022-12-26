using Dapper;
using ServiceIntegrationLibrary.Models;
using System.Data;
using System.Text.RegularExpressions;
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

        public async Task<PaymentDetails> Get(string paymentId)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<PaymentDetails>(SqlQueries.GetTransaction, new { paymentId });
        }

        public async Task<IEnumerable<PaymentDetails>> Get(long merchantId, DateTime from, DateTime to)
        {
            return await _dbConnection.QueryAsync<PaymentDetails>(SqlQueries.GetTransactionsFromMerchant, new { merchantId, from, to });
        }

        public async Task Post(PaymentDetails payment)
        {
            payment.CreditCardNumber = MaskCreditCardNumber(payment.CreditCardNumber);
            payment.Cvv = MaskCvv(payment.Cvv);
            payment.CreatedAt = DateTime.UtcNow;
            await _dbConnection.ExecuteAsync(SqlQueries.PostTransaction, new { payment.PaymentId, payment.MerchantId, payment.CreditCardNumber, payment.ExpirationDate, payment.Cvv, payment.Currency, payment.Amount, payment.CreatedAt, Status = payment.Status.ToString() });
        }

        public async Task UpdateIfExistsElseInsert(PaymentDetails payment)
        {
            //We plan to add encryption on the db level instead of saving the credit card information like this.
            payment.CreditCardNumber = MaskCreditCardNumber(payment.CreditCardNumber);
            payment.Cvv = MaskCvv(payment.Cvv);
            payment.CreatedAt = DateTime.UtcNow;
            await _dbConnection.ExecuteAsync(SqlQueries.UpdateIfExistsElseInsert, new { payment.PaymentId, payment.MerchantId, payment.CreditCardNumber, payment.ExpirationDate, payment.Cvv, payment.Currency, payment.Amount, payment.CreatedAt, Status = payment.Status.ToString() });
        }

        private string MaskCreditCardNumber(string creditCardNumber)
        {
            var lastDigits = creditCardNumber.Substring(creditCardNumber.Length - 4, 4);
            var requiredMask = new String('X', creditCardNumber.Length - lastDigits.Length);
            var maskedString = string.Concat(requiredMask, lastDigits);

            //Gives better-looking formatting (XXXX XXXX XXXX 1111) for the most common card patterns. 
            //We will expand this functionality to all card patterns in the future.
            if (creditCardNumber.Length % 4 == 0)
                maskedString = Regex.Replace(maskedString, ".{4}", "$0 ").Trim();

            return maskedString;
        }

        private string MaskCvv(string cvv)
        {
            return new String('*', cvv.Length);
        }
    }
}
