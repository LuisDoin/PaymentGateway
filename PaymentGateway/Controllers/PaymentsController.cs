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
        /// <returns> The state of the account after the transaction</returns>
        /// <remarks>
        /// </remarks>
        /// <response code="200">The state of the account after the transaction</response>
        [HttpPost("purchase")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "tier2")]
        public IActionResult Payment([FromBody] PaymentDetails paymentDetails)
        {
            try
            {
                _paymentService.validatePayment(paymentDetails);

                //TODO: chamar serviço que põe na fila (esse serviço tem que criar um UUID pra o payment).

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error message: " + ex.Message + " StackTrace: " + ex.StackTrace);
                return StatusCode(500);
            }
        }
    }
}
