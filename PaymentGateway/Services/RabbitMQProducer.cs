using PaymentGateway.Controllers;
using RabbitMQ.Client;
using System.Text;

namespace PaymentGateway.Services
{
    public class RabbitMQProducer : RabbitMQProducerBase
    {
        private readonly IModel _pendingTransactionsChannel;
        private readonly ILogger<RabbitMQProducer> _logger;

        public RabbitMQProducer(IConnection connection, ILogger<RabbitMQProducer> logger) : base(connection)
        {
            _pendingTransactionsChannel = connection.CreateModel();
            _pendingTransactionsChannel.QueueDeclare("pending_transactions", durable: true, exclusive: false, autoDelete: false, arguments: null);
            _logger = logger;
        }

        public override void Publish(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogError("Message ill formed by the system");
                throw new Exception("Message ill formed by the system");
            }

            var body = Encoding.UTF8.GetBytes(message);
            _pendingTransactionsChannel.BasicPublish("", "pending_transactions", null, body);
            _logger.LogInformation("Published message: " + message );
        }
    }
}
