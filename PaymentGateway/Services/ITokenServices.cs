using PaymentGateway.Models;

namespace PaymentGateway.Services
{
    public interface ITokenService
    {
        public Task<string> GenerateToken(User user);
    }
}
