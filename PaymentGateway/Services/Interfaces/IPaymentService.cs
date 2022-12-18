using Model;

namespace PaymentGateway.Services.Interfaces
{
    public interface IPaymentService
    {
        public void validatePayment(PaymentDetails purchaseDetails);
    }
}
