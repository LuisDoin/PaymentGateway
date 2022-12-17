using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Models;
using PaymentGateway.Services.Interfaces;

namespace PaymentGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService purchaseService,
                                      ILogger<PaymentsController> logger)
        {
            _paymentService = purchaseService;
            _logger = logger;
        }

        /// <summary>
        /// Performs a purchase.  
        /// </summary>
        /// <returns> </returns>
        /// <remarks>
        /// Input Info: expirationDate must be in the format: mm/yyyy. List of supported currencies: USD, EUR, GBP, JPY, CNY, AUD, CAD, CHF, HKD, SGD.
        /// </remarks>
        /// <response code="200"></response>
        [HttpPost("payment")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "tier2")]
        public IActionResult Payment([FromBody] PaymentDetails paymentDetails)
        {
            try
            {
                paymentDetails.PaymentId = Guid.NewGuid();

                _logger.LogInformation("Processing payment", paymentDetails);

                _paymentService.validatePayment(paymentDetails);

                //TODO: chamar serviço que põe na fila.

                return Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogInformation("Payment " + paymentDetails.PaymentId + " returned as a BadRequest");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Payment " + paymentDetails.PaymentId + " returned an error. Error message: " + ex.Message + " StackTrace: " + ex.StackTrace);
                return StatusCode(500);
            }
        }
    }
}
