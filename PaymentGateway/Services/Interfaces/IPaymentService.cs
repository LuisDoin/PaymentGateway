using PaymentGateway.Models;

namespace PaymentGateway.Services.Interfaces
{
    public interface IPaymentService
    {
        public void validatePayment(PaymentDetails purchaseDetails);
    }
}
