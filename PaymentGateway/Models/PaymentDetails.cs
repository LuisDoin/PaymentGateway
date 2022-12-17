using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace PaymentGateway.Models
{
    public class PaymentDetails
    {
        public PaymentDetails(string email, string creditCardNumber, string expirationDate, string cvv, string currency, decimal amount)
        {
            Email = email;
            CreditCardNumber = creditCardNumber;
            ExpirationDate = expirationDate;
            Cvv = cvv;
            Currency = currency;
            Amount = amount;
        }

        [JsonIgnore]
        public Guid PaymentId { get; set; }
        public string Email { get; set; }
        public string CreditCardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string Cvv { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
    }
}
