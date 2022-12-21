using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PaymentGateway.Config;
using PaymentProcessor.Config;
using ServiceIntegrationLibrary.Models;
using ServiceIntegrationLibrary.ModelValidationServices;
using ServiceIntegrationLibrary.Utils;
using ServiceIntegrationLibrary.Utils.Interfaces;
using System.Text.RegularExpressions;
using System.Web;

namespace PaymentGateway.Controllers
{
    [ApiController]
    [Route("PaymentGateway/[controller]")]
    public class PaymentsController : Controller
    {
        private readonly IPaymentValidationService _paymentValidationService;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly ILogger<PaymentsController> _logger;
        private readonly IHttpClientProvider _httpClientProvider;
        private readonly RabbitMQSettings _rabbitMQSettings;
        private readonly TransactionsApiSettings _transactionsApiSettings;

        public PaymentsController(IPaymentValidationService paymentValidationService,
            ISendEndpointProvider sendEndpointProvider,
            ILogger<PaymentsController> logger,
            IOptions<RabbitMQSettings> rabbitMQOptions,
            IOptions<TransactionsApiSettings> transactionsApiOptions,
            IHttpClientProvider httpClientProvider)
        {
            _paymentValidationService = paymentValidationService;
            _sendEndpointProvider = sendEndpointProvider;
            _logger = logger;
            _rabbitMQSettings = rabbitMQOptions.Value;
            _transactionsApiSettings = transactionsApiOptions.Value;
            _httpClientProvider = httpClientProvider;
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
                paymentDetails.Status = PaymentStatus.Processing;
                paymentDetails.CreditCardNumber = Regex.Replace(paymentDetails.CreditCardNumber, "[- ]", String.Empty); //Remove spaces and hyphens

                _logger.LogInformation($"Processing payment {paymentDetails}");

                _paymentValidationService.ValidatePayment(paymentDetails);

                var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{_rabbitMQSettings.PendingTransactionsQueue}"));
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

        [HttpPost("paymentResponse")]
        //[Authorize(AuthenticationSchemes = "Bearer", Roles = "tier2")]
        public async Task PaymentResponse([FromBody] PaymentDetails paymentDetails)
        {
            var isSuccessfulPayment = paymentDetails.Status== PaymentStatus.Successful;
            var message = isSuccessfulPayment ? "Pagamento aprovado! :D" :
                                                "Pagamento não aprovado :(";

            //Send
            //to UI;
        }

        [HttpGet("payment")]
        //[Authorize(AuthenticationSchemes = "Bearer", Roles = "tier2")]
        public async Task<IActionResult> Payment(string paymentId)
        {
            try
            {
                _logger.LogInformation($"Fetching payment {paymentId}");

                var builder = new UriBuilder(_transactionsApiSettings.Uri);
                var query = HttpUtility.ParseQueryString(builder.Query);
                query["paymentId"] = paymentId;
                builder.Query = query.ToString();
                string url = builder.ToString();

                using var httpResponseMessage = await _httpClientProvider.GetAsync(url);
                var payment = await httpResponseMessage.Content.ReadAsAsync<Payment>();

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
