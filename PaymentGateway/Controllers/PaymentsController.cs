using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Model;
using Model.ModelValidationServices;
using PaymentProcessor.Config;

namespace PaymentGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentsController : Controller
    {
        private readonly IPaymentValidationService _paymentValidationService;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly ILogger<PaymentsController> _logger;
        private readonly RabbitMQSettings _rabbitMQSettings;

        public PaymentsController(IPaymentValidationService paymentValidationService, 
            ISendEndpointProvider sendEndpointProvider, 
            ILogger<PaymentsController> logger, 
            IOptions<RabbitMQSettings> options)
        {
            _paymentValidationService = paymentValidationService;
            _sendEndpointProvider = sendEndpointProvider;
            _logger = logger;
            _rabbitMQSettings = options.Value;
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

                _logger.LogInformation($"Processing payment {paymentDetails}");

                _paymentValidationService.ValidatePayment(paymentDetails);

                var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{_rabbitMQSettings.pendingTransactionsQueue}"));
                await sendEndpoint.Send(paymentDetails);

                return Ok(paymentDetails.PaymentId);
            }
            catch (ArgumentException ex)
            {
                _logger.LogInformation(ex, $"Payment {paymentDetails.PaymentId} returned as a BadRequest");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Payment {paymentDetails.PaymentId} returned an error");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
