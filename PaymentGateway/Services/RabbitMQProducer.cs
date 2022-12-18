using RabbitMQ.Client;
using System.Text;

namespace PaymentGateway.Services
{
    public class RabbitMQProducer : RabbitMQProducerBase
    {
        private readonly IModel _pendingTransactionsChannel;

        public RabbitMQProducer(IConnection connection) : base(connection)
        {
            _pendingTransactionsChannel = connection.CreateModel();
            _pendingTransactionsChannel.QueueDeclare("pending_transactions", durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        public override void Publish(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new Exception("Message ill formed by the system");

            var body = Encoding.UTF8.GetBytes(message);
            _pendingTransactionsChannel.BasicPublish("", "pending_transactions", null, body);
        }
    }
}
