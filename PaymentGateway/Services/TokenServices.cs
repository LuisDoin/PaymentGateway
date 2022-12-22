using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.IdentityModel.Tokens;
using PaymentGateway.Data.Repositories;
using PaymentGateway.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PaymentGateway.Services
{
    public class TokenService : ITokenService
    {

        private readonly IUserRepository _userRepository;

        public TokenService(IUserRepository usersRepository)
        {
            _userRepository = usersRepository;
        }

        public async Task<string> GenerateToken(string login, string password)
        {
            var user = _userRepository.Get(login, password);

            if (user == null)
                throw new InvalidOperationException("Invalid user or password.");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? "secretUsedForTesting");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Login.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
