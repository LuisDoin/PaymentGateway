using CKOBankSimulator.Models;
using CKOBankSimulator.Services.Interfaces;

namespace CKOBankSimulator.Services
{
    public class PaymentService : IPaymentService
    {
        public void ValidatePaymentInfo(PaymentInfo payment)
        {
            //In a real-world scenario, this method would provide validation of the input.
            //We did not implement it here to avoid repetition of the validation made on PaymentGateway.
            return;
        }

        public bool IsThereEnoughCredditLimit(PaymentInfo payment)
        {
            var availableLimit = GetCardCreditLimit(payment.CreditCardNumber);
            return availableLimit >= payment.Amount;
        }

        private Decimal GetCardCreditLimit(string creditCardNumber)
        {
            //Simulates operation of getting card's available limit.
            return new Random().Next(100);
        }

        public void ProcessPayment(PaymentInfo payment)
        {
            //Not implemented since this is out of the scope of this challenge. 
        }
    }
}
