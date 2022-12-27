using AutoFixture;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using PaymentGateway.Config;
using PaymentGateway.Controllers;
using PaymentGateway.Data.Repositories;
using PaymentGateway.Models;
using PaymentProcessor.Config;
using ServiceIntegrationLibrary.Models;
using ServiceIntegrationLibrary.ModelValidationServices;
using ServiceIntegrationLibrary.Utils.Interfaces;
using System.Collections;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;

namespace PaymentGateway.UnitTests.Controllers
{
    [TestFixture]
    class PaymentsControllerTests
    {
        private Fixture _fixture;
        private Mock<IHttpClientProvider> _httpClientProvider;
        private Mock<IPaymentValidationService> _paymentValidationServiceMock;
        private Mock<IUserRepository> _userRepository;
        private Mock<ILogger<PaymentsController>> _loggerMock;
        private Mock<ISendEndpointProvider> _sendEndpointProviderMock;
        private Mock<ISendEndpoint> _sendEndpointMock;
        private PaymentsController _paymentsController;
        private PaymentDetails _paymentDetails;
        private RabbitMQSettings _rabbitMQSettings;
        private TransactionsApiSettings _transactionsApiSettings;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _paymentValidationServiceMock = new Mock<IPaymentValidationService>();
            _userRepository = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger<PaymentsController>>();
            _sendEndpointProviderMock = new Mock<ISendEndpointProvider>();
            _sendEndpointMock = new Mock<ISendEndpoint>();
            _httpClientProvider = new Mock<IHttpClientProvider>();
            _rabbitMQSettings = new RabbitMQSettings()
            {
                Uri = "amqp://guest:guest@localhost:5672",
                PendingTransactionsQueue = "pending-transactions"
            };
            _transactionsApiSettings = new TransactionsApiSettings()
            {
                GetPaymentUri = "https://localhost:44370/TransactionsApi/Payments/payment",
                GetPaymentsUri = "https://localhost:44370/TransactionsApi/Payments/payments"
            };
            _paymentsController = new PaymentsController(_paymentValidationServiceMock.Object, _userRepository.Object, _sendEndpointProviderMock.Object, _loggerMock.Object, Options.Create(_rabbitMQSettings), Options.Create(_transactionsApiSettings), _httpClientProvider.Object);
            _paymentDetails = new PaymentDetails(_fixture.Create<string>(), 1, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<decimal>());
            _sendEndpointProviderMock.Setup(x => x.GetSendEndpoint(new Uri($"queue:{_rabbitMQSettings.PendingTransactionsQueue}"))).ReturnsAsync(_sendEndpointMock.Object);
            _userRepository.Setup(x => x.Get("Amazon")).Returns(new User() { UserId = 1, Login = "Amazon", Password = "AWSSecret1", Role = "Tier1" });
        }

        [Test]
        public async Task PostPayment_ProcessSuccessfully_ReturnsOk()
        {
            var result = await _paymentsController.Payment(_paymentDetails);
            var okResult = result as OkObjectResult;


            _paymentValidationServiceMock.Verify(x => x.ValidatePayment(_paymentDetails), Times.Once);
            _httpClientProvider.Verify(x => x.PostAsync(_transactionsApiSettings.PostPaymentUri, It.IsAny<string>()), Times.Once);
            _sendEndpointProviderMock.Verify(x => x.GetSendEndpoint(new Uri($"queue:{_rabbitMQSettings.PendingTransactionsQueue}")), Times.Once);
            _sendEndpointMock.Verify(x => x.Send(_paymentDetails, default), Times.Once);
            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            Assert.That(okResult.Value, Is.EqualTo(_paymentDetails.PaymentId));
        }

        [Test]
        public async Task PostPayment_ValidationFail_ReturnBadRequest()
        {
            _paymentValidationServiceMock.Setup(x => x.ValidatePayment(It.IsAny<PaymentDetails>())).Throws<ArgumentException>();

            var result = await _paymentsController.Payment(_paymentDetails);
            var badRequestResult = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task GetPayment_PaymentExists_ReturnsOk()
        {
            var paymentId = Guid.NewGuid().ToString();
            var parameters = new Dictionary<string, string>
            {
                { "paymentId", paymentId },
            };
            var fetchedPayment = JsonConvert.SerializeObject(_paymentDetails);
            var content = new StringContent(fetchedPayment);
            content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);

            _httpClientProvider.Setup(x => x.GetAsync(new Uri(QueryHelpers.AddQueryString(_transactionsApiSettings.GetPaymentUri, parameters)).ToString())).ReturnsAsync(new HttpResponseMessage() { Content = content  });

            var result = await _paymentsController.Payment(paymentId);
            var okResult = result as OkObjectResult;

            _httpClientProvider.Verify(x => x.GetAsync(new Uri(QueryHelpers.AddQueryString(_transactionsApiSettings.GetPaymentUri, parameters)).ToString()), Times.Once);
            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            Assert.That(JsonConvert.SerializeObject(okResult.Value), Is.EqualTo(fetchedPayment));
        }

        [Test]
        public async Task GetPayment_PaymentDoesNotExist_ReturnsNotFound()
        {
            var paymentId = Guid.NewGuid().ToString();
            var parameters = new Dictionary<string, string>
            {
                { "paymentId", paymentId },
            };

            _httpClientProvider.Setup(x => x.GetAsync(new Uri(QueryHelpers.AddQueryString(_transactionsApiSettings.GetPaymentUri, parameters)).ToString())).ReturnsAsync(new HttpResponseMessage() { Content = null });

            var result = await _paymentsController.Payment(paymentId);
            var notFoundResult = result as NotFoundResult;

            _httpClientProvider.Verify(x => x.GetAsync(new Uri(QueryHelpers.AddQueryString(_transactionsApiSettings.GetPaymentUri, parameters)).ToString()), Times.Once);
            Assert.IsNotNull(notFoundResult);
            Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task GetPayments_ProcessSuccessfully_ReturnsOk()
        {
            var from = new DateTime(2000, 1, 1);
            var to = new DateTime(2001, 1, 1);
            var parameters = new Dictionary<string, string>
                {
                    { "merchantId", "1" },
                    { "from", from.ToString("O") },
                    { "to", to.ToString("O") },
                };
            var fetchedPayments = JsonConvert.SerializeObject(new List<PaymentDetails>() { _paymentDetails });
            var content = new StringContent(fetchedPayments);
            content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);

            _httpClientProvider.Setup(x => x.GetAsync(new Uri(QueryHelpers.AddQueryString(_transactionsApiSettings.GetPaymentsUri, parameters)).ToString())).ReturnsAsync(new HttpResponseMessage() { Content = content });

            var result = await _paymentsController.Payments(from, to);
            var okResult = result as OkObjectResult;

            _httpClientProvider.Verify(x => x.GetAsync(new Uri(QueryHelpers.AddQueryString(_transactionsApiSettings.GetPaymentsUri, parameters)).ToString()), Times.Once);
            Assert.IsNotNull(okResult);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            Assert.That(JsonConvert.SerializeObject(okResult.Value), Is.EqualTo(fetchedPayments));
        }
    }
}
