using ServiceIntegrationLibrary.Models;
using ServiceIntegrationLibrary.Utils;

namespace TransactionsApi.Models
{
    public class Payment
    {
        public Payment(PaymentDetails paymentDetails)
        {
            PaymentId = paymentDetails.PaymentId;
            MerchantId = paymentDetails.MerchantId;
            CreditCardNumber = paymentDetails.CreditCardNumber;
            ExpirationDate = paymentDetails.ExpirationDate;
            Cvv = paymentDetails.Cvv;
            Currency = paymentDetails.Currency;
            Amount = paymentDetails.Amount;
            CreationDate = DateTime.UtcNow;
            Status = paymentDetails.Status;
        }

        public Guid PaymentId { get; set; }
        public long MerchantId { get; set; }
        public string CreditCardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string Cvv { get; set; }
        public string Currency { get; set; }
        public Decimal Amount { get; set; }
        public DateTime CreationDate { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
