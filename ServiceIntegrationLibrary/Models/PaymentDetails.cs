using ServiceIntegrationLibrary.Utils;
using System;
using System.Text.Json.Serialization;

namespace ServiceIntegrationLibrary.Models
{
    public class PaymentDetails
    {
        public PaymentDetails(string creditCardNumber, long merchantId, string expirationDate, string cvv, string currency, decimal amount)
        {
            CreditCardNumber = creditCardNumber;
            MerchantId = merchantId;
            ExpirationDate = expirationDate;
            Cvv = cvv;
            Currency = currency;
            Amount = amount;
        }

        public Guid PaymentId { get; set; }
        public long MerchantId { get; set; }
        public string CreditCardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string Cvv { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
