using CKOBankSimulator.Models;

namespace CKOBankSimulator.Services.Interfaces
{
    public interface IPaymentService
    {
        public void ValidatePaymentInfo(PaymentInfo payment);

        public bool IsThereEnoughCredditLimit(PaymentInfo payment);

        public void ProcessPayment(PaymentInfo payment);
    }
}
