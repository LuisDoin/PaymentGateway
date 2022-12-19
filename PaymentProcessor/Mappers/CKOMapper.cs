using Model;
using PaymentProcessor.DTOs;
using PaymentProcessor.Mappers.Interfaces;

namespace PaymentProcessor.Mappers
{
    public class CKOMapper : ICKOMapper
    {
        public CKOPaymentInfoDTO ToDto(PaymentDetails paymentDetails)
        {
            var ckoPaymentInfoDTO = new CKOPaymentInfoDTO
            {
                PaymentId = paymentDetails.PaymentId,
                CreditCardNumber = paymentDetails.CreditCardNumber,
                ExpirationDate = paymentDetails.ExpirationDate,
                Cvv = paymentDetails.Cvv,
                Currency = paymentDetails.Currency,
                Amount = paymentDetails.Amount,
            };

            return ckoPaymentInfoDTO;
        }
    }
}
