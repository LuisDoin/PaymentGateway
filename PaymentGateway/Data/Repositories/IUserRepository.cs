using PaymentGateway.Models;

namespace PaymentGateway.Data.Repositories
{
    public interface IUserRepository
    {
        public User Get(string login, string password);
    }
}
