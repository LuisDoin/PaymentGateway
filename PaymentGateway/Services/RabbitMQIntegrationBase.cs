using PaymentGateway.Services.Interfaces;
using RabbitMQ.Client;
using System.Data.Common;

namespace PaymentGateway.Services
{
    public abstract class RabbitMQIntegrationBase : IQueueIntegrationService
    {

        private readonly IConnection _connection;

        protected RabbitMQIntegrationBase(IConnection connection)
        {
            _connection = connection;
        }

        public abstract void Publish(string message);
    }
}
