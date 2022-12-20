using CKOBankSimulator.Controllers;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CKOBankSimulator.Services.Interfaces;
using CKOBankSimulator.Models;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

namespace CKOBankSimulatorTests.Controllers
{
    [TestFixture]
    class PaymentsControllerTests
    {
        private Fixture _fixture;
        private Mock<IPaymentService> _paymentServiceMock;
        private Mock<ILogger<PaymentsController>> _loggerMock;
        private Mock<ICachingService> _cacheMock;
        private PaymentsController _paymentsController;
        private PaymentInfo _paymentInfo;
        private string _stringPaymentId;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _paymentServiceMock = new Mock<IPaymentService>();
            _loggerMock = new Mock<ILogger<PaymentsController>>();
            _cacheMock = new Mock<ICachingService>();
            _paymentsController = new PaymentsController(_paymentServiceMock.Object, _loggerMock.Object, _cacheMock.Object);
            _paymentInfo = new PaymentInfo(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<decimal>());
            _paymentInfo.PaymentId= Guid.NewGuid();
            _stringPaymentId = _paymentInfo.PaymentId.ToString();
        }

        [Test]
        public async Task Payment_HitsTheCache_ReturnsOkImmediately()
        {
            _cacheMock.Setup(c => c.Contains(_stringPaymentId)).ReturnsAsync(true);

            var result = await _paymentsController.Payment(_paymentInfo);
            var okResult = result as OkObjectResult;

            _cacheMock.Verify(x => x.Contains(_stringPaymentId), Times.Once);
            _paymentServiceMock.Verify(x => x.IsThereEnoughCredditLimit(_paymentInfo), Times.Never);
            _paymentServiceMock.Verify(x => x.ProcessPayment(_paymentInfo), Times.Never);
            _cacheMock.Verify(x => x.SetAsync(_stringPaymentId, It.IsAny<string>()), Times.Never);

            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            Assert.That(okResult.Value, Is.EqualTo(_paymentInfo.PaymentId));
        }

        [Test]
        public async Task Payment_DoesNotHitTheCacheAndHasSufficientFunds_ReturnsOkAndSavesOnCache()
        {
            _cacheMock.Setup(c => c.Contains(_stringPaymentId)).ReturnsAsync(false);
            _paymentServiceMock.Setup(c => c.IsThereEnoughCredditLimit(_paymentInfo)).Returns(true);

            var result = await _paymentsController.Payment(_paymentInfo);
            var okResult = result as OkObjectResult;

            _cacheMock.Verify(x => x.Contains(_stringPaymentId), Times.Once);
            _paymentServiceMock.Verify(x => x.IsThereEnoughCredditLimit(_paymentInfo), Times.Once);
            _paymentServiceMock.Verify(x => x.ProcessPayment(_paymentInfo), Times.Once);
            _cacheMock.Verify(x => x.SetAsync(_stringPaymentId, It.IsAny<string>()), Times.Once);

            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            Assert.That(okResult.Value, Is.EqualTo(_paymentInfo.PaymentId));
        }

        [Test]
        public async Task Payment_DoesNotHitTheCacheAndDoesNotHaveSufficientFunds_ReturnsBadRequest()
        {
            _cacheMock.Setup(c => c.Contains(_stringPaymentId)).ReturnsAsync(false);
            _paymentServiceMock.Setup(c => c.IsThereEnoughCredditLimit(_paymentInfo)).Returns(false);

            var result = await _paymentsController.Payment(_paymentInfo);
            var okResult = result as BadRequestObjectResult;

            _cacheMock.Verify(x => x.Contains(_stringPaymentId), Times.Once);
            _paymentServiceMock.Verify(x => x.IsThereEnoughCredditLimit(_paymentInfo), Times.Once);
            _paymentServiceMock.Verify(x => x.ProcessPayment(_paymentInfo), Times.Never);
            _cacheMock.Verify(x => x.SetAsync(_stringPaymentId, It.IsAny<string>()), Times.Never);

            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(400));
        }
    }
}
