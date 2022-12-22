using Microsoft.AspNetCore.Mvc;
using ServiceIntegrationLibrary.Models;
using TransactionsApi.Data.Repositories;

namespace TransactionsApi.Controllers
{
    [Route("TransactionsApi/[controller]")]
    public class PaymentsController : Controller
    {
        private readonly ILogger<PaymentsController> _logger;
        private readonly IPaymentsRepository _paymentsRepository;

        public PaymentsController(ILogger<PaymentsController> logger, IPaymentsRepository paymentsRepository)
        {
            _logger = logger;
            _paymentsRepository = paymentsRepository;
        }

        /// <summary>
        /// Performs a purchase.  
        /// </summary>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        /// <response code="200"></response>
        [HttpGet("payment")]
        public async Task<IActionResult> Payment(string paymentId)
        {
            try
            {
                _logger.LogInformation($"Fetching payment {paymentId}");
                var payment = await _paymentsRepository.Get(paymentId);

                return payment != null ? Ok(payment) : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while fetching payment {paymentId}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Performs a purchase.  
        /// </summary>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        /// <response code="200"></response>
        [HttpGet("payments")]
        public async Task<IActionResult> Payments(long merchantId, DateTime from, DateTime? to = null)
        {
            try
            {
                var payments = await _paymentsRepository.Get(merchantId, from, to);

                return payments != null ? Ok(payments) : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while fetching payments from merchant {merchantId}");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Performs a purchase.  
        /// </summary>
        /// <returns> </returns>
        /// <remarks>
        /// </remarks>
        /// <response code="200"></response>
        [HttpPost("payment")]
        public async Task<IActionResult> Payment([FromBody] PaymentDetails paymentDetails)
        {
            try
            {
                await _paymentsRepository.Post(paymentDetails);
                return Ok(paymentDetails.PaymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while posting payment {paymentDetails.PaymentId}");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
