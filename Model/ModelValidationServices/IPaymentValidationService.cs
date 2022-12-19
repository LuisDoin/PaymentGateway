using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ModelValidationServices
{
    public interface IPaymentValidationService
    {
        void ValidatePayment(PaymentDetails purchaseDetails);
    }
}
