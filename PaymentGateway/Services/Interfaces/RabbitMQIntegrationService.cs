using RabbitMQ.Client;
using System.Text;

namespace PaymentGateway.Services.Interfaces
{
    public class RabbitMQIntegrationService : RabbitMQIntegrationBase
    {
        private readonly IModel _pendingTransactionsChannel;

        public RabbitMQIntegrationService(IConnection connection) : base(connection)
        {
            _pendingTransactionsChannel = connection.CreateModel();
            _pendingTransactionsChannel.QueueDeclare("pending_transactions", durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        public override void Publish(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _pendingTransactionsChannel.BasicPublish("", "pending_transactions", null, body);
        }
    }
}
