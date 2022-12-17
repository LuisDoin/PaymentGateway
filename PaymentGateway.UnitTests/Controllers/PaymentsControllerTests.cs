using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using PaymentGateway.Controllers;
using PaymentGateway.Models;
using PaymentGateway.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentGateway.UnitTests.Controllers
{
    [TestFixture]
    class PaymentsControllerTests
    {
        private Mock<IPaymentService> _paymentServiceMock;
        private Mock<IQueueIntegrationService> _queueIntegrationServiceMock;
        private Mock<ILogger<PaymentsController>> _loggerMock;
        private PaymentsController _paymentsController;
        private Fixture _fixture;
        private PaymentDetails _paymentDetails;


        [SetUp]
        public void SetUp()
        {
            _paymentServiceMock = new Mock<IPaymentService>();
            _queueIntegrationServiceMock = new Mock<IQueueIntegrationService>();
            _loggerMock = new Mock<ILogger<PaymentsController>>();
            _paymentsController = new PaymentsController(_paymentServiceMock.Object, _queueIntegrationServiceMock.Object, _loggerMock.Object);
            _fixture = new Fixture();
            _paymentDetails = new PaymentDetails(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<decimal>());
        }

        [Test]
        public void Payment_ValidationFail_ReturnBadRequest()
        {
            _paymentServiceMock.Setup(x => x.validatePayment(It.IsAny<PaymentDetails>())).Throws<ArgumentException>();

            var result = _paymentsController.Payment(_paymentDetails);
            var badRequestResult = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public void Payment_ValidationPass_CallsValidationAndPublishMethods()
        {
            _paymentsController.Payment(_paymentDetails);

            _paymentServiceMock.Verify(x => x.validatePayment(_paymentDetails), Times.Once);
            _queueIntegrationServiceMock.Verify(x => x.Publish(JsonConvert.SerializeObject(_paymentDetails)), Times.Once);
        }
    }
}
