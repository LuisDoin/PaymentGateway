    using CKOBankSimulator.Models;
using CKOBankSimulator.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CKOBankSimulator.Controllers
{
    [Route("CKOBankApi/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {

        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;
        private readonly ICachingService _cache;

        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger, ICachingService cache)
        {
            _paymentService = paymentService;
            _logger = logger;
            _cache = cache;
        }

        [HttpPost("processPayment")]
        public async Task<IActionResult> Payment([FromBody] PaymentInfo paymentInfo)
        {
            try
            {
                _paymentService.ValidatePaymentInfo(paymentInfo);

                _logger.LogInformation($"Processing payment {paymentInfo.PaymentId}");

                var paymentId = paymentInfo.PaymentId.ToString();

                //This check is providing idempotence, preventing us from charging twice for the same purchase. 
                if (await _cache.Contains(paymentId))
                {
                    _logger.LogInformation($"Payment {paymentInfo.PaymentId} already processed");
                    return Ok(paymentInfo.PaymentId);
                }

                if (!_paymentService.IsThereEnoughCredditLimit(paymentInfo))
                {
                    _logger.LogInformation($"Unsuccessful payment: not enough limit. PaymentId: {paymentInfo.PaymentId}");
                    return BadRequest("Not enough limit.");
                }

                _paymentService.ProcessPayment(paymentInfo);

                //Here, we have a hole in our system. If the server crashes after processing the payment but before saving it to the cache,
                //this request will be resent, and this payment will be processed twice. A more involved design is needed to fix this, such
                //as using a state machine or processing both operations separately with idempotent retry mechanisms. Since the purpose
                //of the CKOBankSimulator is to allow us to test our PaymentGatewayin, which is the focus of this challenge,
                //we will leave the implementation of the CKOBankSimulator as is.

                await _cache.SetAsync(paymentId, "Payment successfully processed");

                return Ok(paymentInfo.PaymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Payment {paymentInfo.PaymentId} returned an error");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
