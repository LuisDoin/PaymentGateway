using ServiceIntegrationLibrary.Utils;
using System.Text.Json.Serialization;

namespace ServiceIntegrationLibrary.Models
{
    public class ProcessedPayment
    {
        public ProcessedPayment()
        {
        }

        public ProcessedPayment(IncomingPayment IncomingPayment)
        {
            PaymentId = IncomingPayment.PaymentId;
            MerchantId = IncomingPayment.MerchantId;
            CreditCardNumber = IncomingPayment.CreditCardNumber;
            ExpirationDate = IncomingPayment.ExpirationDate;
            Cvv = IncomingPayment.Cvv;
            Currency = IncomingPayment.Currency;
            Amount = IncomingPayment.Amount;
            Status = IncomingPayment.Status;
            ProcessedAt = DateTime.UtcNow;
        }

        public Guid PaymentId { get; set; }
        public long MerchantId { get; set; }
        public string CreditCardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string Cvv { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentStatus Status { get; set; }

        public DateTime ProcessedAt { get; set; }
    }
}
