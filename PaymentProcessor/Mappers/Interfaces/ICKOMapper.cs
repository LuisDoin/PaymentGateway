using PaymentProcessor.DTOs;
using ServiceIntegrationLibrary.Models;

namespace PaymentProcessor.Mappers.Interfaces
{
    public interface ICKOMapper
    {
        public CKOPaymentInfoDTO ToDto(PaymentDetails IncomingPayment);
    }
}
