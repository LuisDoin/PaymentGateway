using Newtonsoft.Json;

namespace PaymentGateway.Models
{
    public class User
    {
        [JsonIgnore]
        public long UserId { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        [JsonIgnore]
        public string Role { get; set; }
    }
}
