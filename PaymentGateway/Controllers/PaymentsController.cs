using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using PaymentGateway.Config;
using PaymentGateway.Data.Repositories;
using PaymentProcessor.Config;
using ServiceIntegrationLibrary.Models;
using ServiceIntegrationLibrary.ModelValidationServices;
using ServiceIntegrationLibrary.Utils;
using ServiceIntegrationLibrary.Utils.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web.Http;
using AuthorizeAttribute = Microsoft.AspNetCore.Authorization.AuthorizeAttribute;
using FromBodyAttribute = Microsoft.AspNetCore.Mvc.FromBodyAttribute;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace PaymentGateway.Controllers
{
    [ApiController]
    [Route("PaymentGateway/[controller]")]
    public class PaymentsController : Controller
    {
        private readonly IPaymentValidationService _paymentValidationService;
        private readonly IUserRepository _userRepository;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly ILogger<PaymentsController> _logger;
        private readonly IHttpClientProvider _httpClientProvider;
        private readonly RabbitMQSettings _rabbitMQSettings;
        private readonly TransactionsApiSettings _transactionsApiSettings;

        public PaymentsController(IPaymentValidationService paymentValidationService,
            IUserRepository userRepository,
            ISendEndpointProvider sendEndpointProvider,
            ILogger<PaymentsController> logger,
            IOptions<RabbitMQSettings> rabbitMQOptions,
            IOptions<TransactionsApiSettings> transactionsApiOptions,
            IHttpClientProvider httpClientProvider)
        {
            _paymentValidationService = paymentValidationService;
            _userRepository = userRepository;
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
        public async Task<IActionResult> Payment([FromBody] PaymentDetails paymentDetails, int delayInSeconds = 0)
        {
            try
            {
                paymentDetails.PaymentId = Guid.NewGuid();
                paymentDetails.Status = PaymentStatus.Processing;
                paymentDetails.MerchantId = GetCurrentUserId();
                paymentDetails.CreditCardNumber = Regex.Replace(paymentDetails.CreditCardNumber, "[- ]", String.Empty); //Remove spaces and hyphens

                _logger.LogInformation($"Processing payment {paymentDetails}");

                _paymentValidationService.ValidatePayment(paymentDetails);

                var paymentJson = JsonSerializer.Serialize(paymentDetails);
                await _httpClientProvider.PostAsync(_transactionsApiSettings.PostPaymentUri, paymentJson);

                Thread.Sleep(1000*delayInSeconds);

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
                _logger.LogInformation($"Fetching payment {paymentId}");

                if (!Guid.TryParse(paymentId, out _))
                {
                    _logger.LogInformation($"Invalid paymentId {paymentId}. PaymentId must be a GUID");
                    return BadRequest("PaymentId must be a GUID.");
                }                

                var parameters = new Dictionary<string, string>
                {
                    { "paymentId", paymentId },
                };

                using var httpResponseMessage = await _httpClientProvider.GetAsync(new Uri(QueryHelpers.AddQueryString(_transactionsApiSettings.GetPaymentUri, parameters)).ToString());
                
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
        public async Task<IActionResult> Payments(DateTime from, DateTime? to = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId(); 

                _logger.LogInformation($"Fetching payments from merchant {currentUserId}");

                if (to == null)
                    to = DateTime.UtcNow;

                if(DateTime.Compare(from, to.Value) > 0)
                {
                    _logger.LogInformation("Invalid dates. 'from' parameter must be prior to 'to' parameter");
                    return BadRequest("Invalid dates. 'from' parameter must be prior to 'to' parameter");
                }

                var parameters = new Dictionary<string, string>
                {
                    { "merchantId", currentUserId.ToString() }, 
                    { "from", from.ToString("O") },
                    { "to", to.Value.ToString("O") },
                };

                using var httpResponseMessage = await _httpClientProvider.GetAsync(new Uri(QueryHelpers.AddQueryString(_transactionsApiSettings.GetPaymentsUri, parameters)).ToString());

                var payments = await httpResponseMessage.Content.ReadAsAsync<IEnumerable<PaymentDetails>>();
                return payments != null ? Ok(payments) : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while fetching payments.");
                return StatusCode(500, ex.Message);
            }
        }

        private string GetCurrentUser()
        {
            //We declare the testing token here since we do not have access to HttpContext in our unit test class.
            var tokenUsedforUnitTesting = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkFtYXpvbiIsInJvbGUiOiJUaWVyMSIsIm5iZiI6MTY3MTczNjA0NywiZXhwIjoxNjcxNzQzMjQ3LCJpYXQiOjE2NzE3MzYwNDd9.nlssiFDzZFv5TtvnFXQRc_iAbFTOZ8qymB_-sEech9Q".Replace("Bearer ", string.Empty);
            var token = HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty) ?? tokenUsedforUnitTesting;
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            return jsonToken.Claims.First(x => x.Type == "unique_name")?.Value;
        }

        private long GetCurrentUserId()
        {
            var currentUser = GetCurrentUser();
            return _userRepository.Get(currentUser).UserId;
        }
    }
}
