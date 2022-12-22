using PaymentGateway.Models;

namespace PaymentGateway.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        //We are only simulating a repository. All data manipulation will be done in memory.
        private IList<User> _users;

        public UserRepository()
        {
            _users = new List<User>() { new User { UserId = 1, Login = "Amazon", Password = "AWSSecret1", Role = "Tier1"},
                                        new User { UserId = 1, Login = "Amazon", Password = "AWSSecret2", Role = "Tier2"},
                                        new User { UserId = 2, Login = "Nike", Password = "NikeSecret1", Role = "Tier1"},
                                        new User { UserId = 2, Login = "Nike", Password = "NikeSecret2", Role = "Tier2"}};
        }

        public User Get(string login, string password)
        {
            return _users.FirstOrDefault<User>(u => u.Login == login && u.Password == password );
        }

        public User Get(string login)
        {
            return _users.FirstOrDefault<User>(u => u.Login == login);
        }
    }
}
