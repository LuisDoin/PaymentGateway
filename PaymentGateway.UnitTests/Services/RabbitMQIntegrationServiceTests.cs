using Moq;
using NUnit.Framework;
using PaymentGateway.Services;
using RabbitMQ.Client;

namespace PaymentGateway.UnitTests.Services
{
    [TestFixture]
    class RabbitMQIntegrationServiceTests
    {
        private RabbitMQIntegrationService _rabbitMQIntegrationService;
        private Mock<IConnection> _connectionMock;

        [SetUp]
        public void SetUp()
        {
            _connectionMock = new Mock<IConnection>();
            _connectionMock.Setup(c => c.CreateModel()).Returns(new Mock<IModel>().Object);
            _rabbitMQIntegrationService = new RabbitMQIntegrationService(_connectionMock.Object);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Publish_NullOrEmptyMessage_ThrowsException(string message)
        {
            Assert.Throws<Exception>(() => _rabbitMQIntegrationService.Publish(message));
        }
    }
}
