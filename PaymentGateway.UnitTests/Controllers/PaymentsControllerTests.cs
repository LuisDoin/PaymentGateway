using AutoFixture;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Model;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using PaymentGateway.Controllers;
using PaymentGateway.Services.Interfaces;

namespace PaymentGateway.UnitTests.Controllers
{
    [TestFixture]
    class PaymentsControllerTests
    {
        private Mock<IPaymentService> _paymentServiceMock;
        private Mock<ILogger<PaymentsController>> _loggerMock;
        private Mock<IPublishEndpoint> _publishEndpointMock;
        private PaymentsController _paymentsController;
        private Fixture _fixture;
        private PaymentDetails _paymentDetails;


        [SetUp]
        public void SetUp()
        {
            _paymentServiceMock = new Mock<IPaymentService>();
            _loggerMock = new Mock<ILogger<PaymentsController>>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _paymentsController = new PaymentsController(_paymentServiceMock.Object, _publishEndpointMock.Object, _loggerMock.Object);
            _fixture = new Fixture();
            _paymentDetails = new PaymentDetails(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<decimal>());
        }

        [Test]
        public void Payment_ValidationFail_ReturnBadRequest()
        {
            _paymentServiceMock.Setup(x => x.validatePayment(It.IsAny<PaymentDetails>())).Throws<ArgumentException>();

            var result = _paymentsController.Payment(_paymentDetails).Result;
            var badRequestResult = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void Payment_ValidationPass_CallsValidationAndPublishMethods()
        {
            var result = _paymentsController.Payment(_paymentDetails).Result;
            var okResult = result as OkObjectResult;

            _paymentServiceMock.Verify(x => x.validatePayment(_paymentDetails), Times.Once);
            _publishEndpointMock.Verify(x => x.Publish(_paymentDetails, default), Times.Once);

            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            Assert.That(okResult.Value, Is.EqualTo(_paymentDetails.PaymentId));
        }
    }
}
