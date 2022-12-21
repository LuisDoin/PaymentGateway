using Microsoft.AspNetCore.Mvc;
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
        /// Input Info: expirationDate must be in the format: mm/yyyy. List of supported currencies: USD, EUR, GBP, JPY, CNY, AUD, CAD, CHF, HKD, SGD. Valid credit card and cvv for testing: 4324781866717289 and 000.
        /// </remarks>
        /// <response code="200"></response>
        [HttpGet("payment")]
        //[Authorize(AuthenticationSchemes = "Bearer", Roles = "tier2")]
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
    }
}
