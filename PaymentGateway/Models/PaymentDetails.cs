namespace PaymentGateway.Models
{
    public class PaymentDetails
    {
        public string Email { get; set; }
        public string Currency { get; set; }
        public string ExpirationDate { get; set; }
        public string CreditCardNumber { get; set; }
        public string Cvv { get; set; }
        public double Amount { get; set; }
    }
}
