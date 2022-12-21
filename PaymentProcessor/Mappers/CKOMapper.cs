using PaymentProcessor.DTOs;
using PaymentProcessor.Mappers.Interfaces;
using ServiceIntegrationLibrary.Models;

namespace PaymentProcessor.Mappers
{
    public class CKOMapper : ICKOMapper
    {
        public CKOPaymentInfoDTO ToDto(PaymentDetails incomingPayment)
        {
            var ckoPaymentInfoDTO = new CKOPaymentInfoDTO
            {
                PaymentId = incomingPayment.PaymentId,
                CreditCardNumber = incomingPayment.CreditCardNumber,
                ExpirationDate = incomingPayment.ExpirationDate,
                Cvv = incomingPayment.Cvv,
                Currency = incomingPayment.Currency,
                Amount = incomingPayment.Amount,
            };

            return ckoPaymentInfoDTO;
        }
    }
}
