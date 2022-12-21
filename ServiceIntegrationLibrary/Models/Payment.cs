using ServiceIntegrationLibrary.Utils;
using System.Text.Json.Serialization;

namespace ServiceIntegrationLibrary.Models
{
    public class Payment
    {
        public Payment()
        {
        }

        public Payment(PaymentDetails paymentDetails)
        {
            PaymentId = paymentDetails.PaymentId;
            MerchantId = paymentDetails.MerchantId;
            CreditCard = paymentDetails.CreditCardNumber;
            ExpirationDate = paymentDetails.ExpirationDate;
            Cvv = paymentDetails.Cvv;
            Currency = paymentDetails.Currency;
            Amount = paymentDetails.Amount;
            CreationDate = DateTime.UtcNow;
            Status = paymentDetails.Status;
        }

        public Guid PaymentId { get; set; }
        public long MerchantId { get; set; }
        public string CreditCard { get; set; }
        public string ExpirationDate { get; set; }
        public string Cvv { get; set; }
        public string Currency { get; set; }
        public Decimal Amount { get; set; }
        public DateTime CreationDate { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentStatus Status { get; set; }
    }
}
