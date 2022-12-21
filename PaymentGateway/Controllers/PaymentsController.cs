using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using PaymentGateway.Config;
using PaymentProcessor.Config;
using ServiceIntegrationLibrary.Models;
using ServiceIntegrationLibrary.ModelValidationServices;
using ServiceIntegrationLibrary.Utils;
using ServiceIntegrationLibrary.Utils.Interfaces;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using FromBodyAttribute = Microsoft.AspNetCore.Mvc.FromBodyAttribute;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using AuthorizeAttribute = Microsoft.AspNetCore.Authorization.AuthorizeAttribute;
using PaymentGateway.Services;
using System.Security.Policy;
using System.Text.Json;

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
        /// </summary>
        /// <returns> </returns>
        /// <remarks>
        /// Input Info: expirationDate must be in the format: mm/yyyy. List of supported currencies: USD, EUR, GBP, JPY, CNY, AUD, CAD, CHF, HKD, SGD. Valid credit card and cvv for testing: 4324781866717289 and 000.
        /// </remarks>
        /// <response code="200"></response>
        [HttpPost("payment")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Tier1")]
        public async Task<IActionResult> Payment([Microsoft.AspNetCore.Mvc.FromBody] PaymentDetails paymentDetails, int delayInSeconds = 0)
        {
            try
            {
                paymentDetails.PaymentId = Guid.NewGuid();
                paymentDetails.Status = PaymentStatus.Processing;
                paymentDetails.MerchantId = CurrentUser.UserId;
                paymentDetails.CreditCardNumber = Regex.Replace(paymentDetails.CreditCardNumber, "[- ]", String.Empty); //Remove spaces and hyphens

                var paymentJson = JsonSerializer.Serialize(paymentDetails);
                await _httpClientProvider.PostAsync(_transactionsApiSettings.PostPaymentUri, paymentJson);

                Thread.Sleep(1000*delayInSeconds);

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

        [HttpGet("payment")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Tier1,Tier2" )]
        public async Task<IActionResult> Payment(string paymentId)
        {
            try
            {
                if(!Guid.TryParse(paymentId, out _))
                    return BadRequest("PaymentId must be a GUID.");

                _logger.LogInformation($"Fetching payment {paymentId}");

                var parameters = new Dictionary<string, string>
                {
                    { "paymentId", paymentId },
                };

                string url = BuildUrlWithParameters(_transactionsApiSettings.GetPaymentUri, parameters);

                using var httpResponseMessage = await _httpClientProvider.GetAsync(url);
                
                var payment = await httpResponseMessage.Content.ReadAsAsync<PaymentDetails>();
                return payment != null ? Ok(payment) : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while fetching payment {paymentId}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("payments")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Tier1,Tier2")]
        public async Task<IActionResult> Payments(long merchantId, DateTime from, DateTime? to = null)
        {
            try
            {
                _logger.LogInformation($"Fetching payments from merchant {merchantId}");

                var parameters = new Dictionary<string, string>
                {
                    { "merchantId", merchantId.ToString() }, 
                    { "from", from.ToString() },
                };
                if (to != null)
                    parameters.Add("to", to.ToString());

                string url = BuildUrlWithParameters(_transactionsApiSettings.GetPaymentsUri, parameters);

                using var httpResponseMessage = await _httpClientProvider.GetAsync(url);

                var payments = await httpResponseMessage.Content.ReadAsAsync<IEnumerable<PaymentDetails>>();
                return payments != null ? Ok(payments) : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while fetching payments from merchant {merchantId}");
                return StatusCode(500, ex.Message);
            }
        }

        private string BuildUrlWithParameters(string uri, Dictionary<string, string> parameters)
        {
            var builder = new UriBuilder(uri);
            var query = HttpUtility.ParseQueryString(builder.Query);
            foreach(var entry in parameters)
                query[entry.Key] = entry.Value;
            builder.Query = query.ToString();
            return builder.ToString();
        }
    }
}
