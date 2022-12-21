using PaymentGateway.Models;

namespace PaymentGateway.Services
{
    public static class CurrentUser
    {
        public static long UserId { get; set; }
        public static string JwtToken { get; set; }
    }
}
