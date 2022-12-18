namespace CKOBankSimulator.Model
{
    public class PaymentInfo
    {
        public Guid PaymentId { get; set; }
        public string Email { get; set; }
        public string CreditCardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string Cvv { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
    }
}
