using Model;
using PaymentProcessor.DTOs;

namespace PaymentProcessor.Mappers.Interfaces
{
    public interface ICKOMapper
    {
        public CKOPaymentInfoDTO ToDto(PaymentDetails paymentDetails);
    }
}
