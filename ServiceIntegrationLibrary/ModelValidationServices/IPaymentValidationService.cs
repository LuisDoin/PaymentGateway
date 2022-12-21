using ServiceIntegrationLibrary.Models;

namespace ServiceIntegrationLibrary.ModelValidationServices
{
    public interface IPaymentValidationService
    {
        void ValidatePayment(PaymentDetails purchaseDetails);
    }
}
