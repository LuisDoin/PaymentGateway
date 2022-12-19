using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.ModelValidationServices;

namespace PaymentGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentsController : Controller
    {
        private readonly IPaymentValidationService _paymentValidationService;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentValidationService paymentValidationService, IPublishEndpoint publishEndpoint, ILogger<PaymentsController> logger)
        {
            _paymentValidationService = paymentValidationService;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        /// <summary>
        /// Performs a purchase.  
        /// </summary>
        /// <returns> </returns>
        /// <remarks>
        /// Input Info: expirationDate must be in the format: mm/yyyy. List of supported currencies: USD, EUR, GBP, JPY, CNY, AUD, CAD, CHF, HKD, SGD. Valid credit card and cvv for testing: 4324781866717289 and 000.
        /// </remarks>
        /// <response code="200"></response>
        [HttpPost("payment")]
        //[Authorize(AuthenticationSchemes = "Bearer", Roles = "tier2")]
        public async Task<IActionResult> Payment([FromBody] PaymentDetails paymentDetails)
        {
            try
            {
                paymentDetails.PaymentId = Guid.NewGuid();

                _logger.LogInformation("Processing payment {PaymentId}", paymentDetails);

                _paymentValidationService.ValidatePayment(paymentDetails);

                await _publishEndpoint.Publish<PaymentDetails>(paymentDetails);

                return Ok(paymentDetails.PaymentId);
            }
            catch (ArgumentException ex)
            {
                _logger.LogInformation(ex, "Payment {PaymentId} returned as a BadRequest", paymentDetails.PaymentId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment {PaymentId} returned an error", paymentDetails.PaymentId);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
