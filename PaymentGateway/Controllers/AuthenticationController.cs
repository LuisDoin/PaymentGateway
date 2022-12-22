using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Services;

namespace PaymentGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly ITokenService _tokenServices;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(ITokenService tokenServices,
                                        ILogger<AuthenticationController> logger)
        {
            _tokenServices = tokenServices;
            _logger = logger;
        }

        /// <summary>        
        /// </summary>
        /// <returns> </returns>
        /// <remarks>
        /// 
        /// Register users: Amazon and Nike. Their passwords are AWSSecret1, AWSSecret2, NikeSecret1 and NikeSecret2. Secret1 provides a Tier1 role with full access and Secret2 provides a Tier2 role with access only to the Get endpoints.  
        /// 
        /// </remarks>
        /// <response code="200"></response>
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<dynamic>> Authenticate(string login, string password)
        {
            try
            {
                var token = await _tokenServices.GenerateToken(login, password);

                return Ok(token);
            }
            catch (InvalidOperationException ex)
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
