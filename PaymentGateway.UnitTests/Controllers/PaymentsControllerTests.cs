using AutoFixture;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Model;
using Model.ModelValidationServices;
using Moq;
using NUnit.Framework;
using PaymentGateway.Controllers;

namespace PaymentGateway.UnitTests.Controllers
{
    [TestFixture]
    class PaymentsControllerTests
    {
        private Mock<IPaymentValidationService> _paymentValidationServiceMock;
        private Mock<ILogger<PaymentsController>> _loggerMock;
        private Mock<IPublishEndpoint> _publishEndpointMock;
        private PaymentsController _paymentsController;
        private Fixture _fixture;
        private PaymentDetails _paymentDetails;


        [SetUp]
        public void SetUp()
        {
            _paymentValidationServiceMock = new Mock<IPaymentValidationService>();
            _loggerMock = new Mock<ILogger<PaymentsController>>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _paymentsController = new PaymentsController(_paymentValidationServiceMock.Object, _publishEndpointMock.Object, _loggerMock.Object);
            _fixture = new Fixture();
            _paymentDetails = new PaymentDetails(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<decimal>());
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

        [Test]
        public void Payment_ValidationPass_CallsValidationAndPublishMethods()
        {
            var result = _paymentsController.Payment(_paymentDetails).Result;
            var okResult = result as OkObjectResult;

            _paymentValidationServiceMock.Verify(x => x.ValidatePayment(_paymentDetails), Times.Once);
            _publishEndpointMock.Verify(x => x.Publish(_paymentDetails, default), Times.Once);

            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            Assert.That(okResult.Value, Is.EqualTo(_paymentDetails.PaymentId));
        }
    }
}
