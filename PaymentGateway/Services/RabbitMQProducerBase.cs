using PaymentGateway.Services.Interfaces;
using RabbitMQ.Client;
using System.Data.Common;

namespace PaymentGateway.Services
{
    public abstract class RabbitMQProducerBase : IQueueProducer
    {

        private readonly IConnection _connection;

        protected RabbitMQProducerBase(IConnection connection)
        {
            _connection = connection;
        }

        public abstract void Publish(string message);
    }
}
