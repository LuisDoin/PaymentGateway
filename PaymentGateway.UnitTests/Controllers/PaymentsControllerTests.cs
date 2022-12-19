using AutoFixture;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model;
using Model.ModelValidationServices;
using Moq;
using NUnit.Framework;
using PaymentGateway.Controllers;
using PaymentProcessor.Config;

namespace PaymentGateway.UnitTests.Controllers
{
    [TestFixture]
    class PaymentsControllerTests
    {
        private Fixture _fixture;
        private Mock<IPaymentValidationService> _paymentValidationServiceMock;
        private Mock<ILogger<PaymentsController>> _loggerMock;
        private Mock<ISendEndpointProvider> _sendEndpointProviderMock;
        private Mock<ISendEndpoint> _sendEndpointMock;
        private PaymentsController _paymentsController;
        private PaymentDetails _paymentDetails;
        private RabbitMQSettings _rabbitmqSettings;


        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _paymentValidationServiceMock = new Mock<IPaymentValidationService>();
            _loggerMock = new Mock<ILogger<PaymentsController>>();
            _sendEndpointProviderMock = new Mock<ISendEndpointProvider>();
            _sendEndpointMock = new Mock<ISendEndpoint>();
            _rabbitmqSettings = new RabbitMQSettings()
            {
                Uri = "amqp://guest:guest@localhost:5672",
                pendingTransactionsQueue = "pending-transactions"
            };
            _paymentsController = new PaymentsController(_paymentValidationServiceMock.Object, _sendEndpointProviderMock.Object, _loggerMock.Object, Options.Create(_rabbitmqSettings));
            _paymentDetails = new PaymentDetails(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<decimal>());
            _sendEndpointProviderMock.Setup(x => x.GetSendEndpoint(new Uri($"queue:{_rabbitmqSettings.pendingTransactionsQueue}"))).ReturnsAsync(_sendEndpointMock.Object);
        }

        [Test]
        public void Payment_ProcessSuccessfully_CallsValidationAndPublishMethodsAndReturnsOk()
        {
            var result = _paymentsController.Payment(_paymentDetails).Result;
            var okResult = result as OkObjectResult;

            _paymentValidationServiceMock.Verify(x => x.ValidatePayment(_paymentDetails), Times.Once);
            _sendEndpointProviderMock.Verify(x => x.GetSendEndpoint(new Uri($"queue:{_rabbitmqSettings.pendingTransactionsQueue}")), Times.Once);
            _sendEndpointMock.Verify(x => x.Send(_paymentDetails, default), Times.Once);

            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            Assert.That(okResult.Value, Is.EqualTo(_paymentDetails.PaymentId));
        }

        [Test]
        public void Payment_ValidationFail_ReturnBadRequest()
        {
            _paymentValidationServiceMock.Setup(x => x.ValidatePayment(It.IsAny<PaymentDetails>())).Throws<ArgumentException>();

            var result = _paymentsController.Payment(_paymentDetails).Result;
            var badRequestResult = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
        }
    }
}
