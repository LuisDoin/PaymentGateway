using AutoFixture;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using PaymentProcessor.Config;
using PaymentProcessor.Consumers;
using PaymentProcessor.DTOs;
using PaymentProcessor.Mappers.Interfaces;
using ServiceIntegrationLibrary.Models;
using ServiceIntegrationLibrary.ModelValidationServices;
using ServiceIntegrationLibrary.Utils.Interfaces;
using System.Text.Json;

namespace PaymentProcessorUnitTests.Consumers
{
    [TestFixture]
    class PaymentConsumerTests
    {
        private Fixture _fixture;
        private InMemoryTestHarness harness;
        private Mock<IHttpClientProvider> _httpClientProviderMock;
        private Mock<IHttpResponseMessageProvider> _httpResponseMessageProviderMock;
        private Mock<IPaymentValidationService> _paymentValidationServiceMock;
        private Mock<ICKOMapper> _ckoMapperMock;
        private Mock<ILogger<PaymentConsumer>> _loggerMock;
        private CKOBankSettings _cKOBankSettings;
        private RabbitMQSettings _rabbitMQSettings;
        private IConsumerTestHarness<PaymentConsumer> _paymentConsumer;
        private IncomingPayment _paymentDetails;
        private CKOPaymentInfoDTO _ckoPaymentInfoDTO;
        private string CKOBankDtotJson;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _httpClientProviderMock = new Mock<IHttpClientProvider>();
            _httpResponseMessageProviderMock = new Mock<IHttpResponseMessageProvider>();
            _paymentValidationServiceMock = new Mock<IPaymentValidationService>();
            _ckoMapperMock = new Mock<ICKOMapper>();
            _loggerMock = new Mock<ILogger<PaymentConsumer>>();
            _cKOBankSettings = new CKOBankSettings()
            {
                Uri = "https://localhost:44326/CKOBankApi/Payments/processPayment",
            };
            _rabbitMQSettings = new RabbitMQSettings()
            {
                Uri = "amqp://guest:guest@localhost:5672",
                PendingTransactionsQueue = "pending-transactions",
                CompletedTransactionsQueue = "complete-transactions"
            };

            _paymentDetails = new IncomingPayment(_fixture.Create<string>(), 1, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<decimal>());
            _ckoPaymentInfoDTO = new CKOPaymentInfoDTO();
            CKOBankDtotJson = JsonSerializer.Serialize(_ckoPaymentInfoDTO);

            _ckoMapperMock.Setup(x => x.ToDto(It.IsAny<IncomingPayment>())).Returns(_ckoPaymentInfoDTO);
            _httpClientProviderMock.Setup(x => x.PostAsync(_cKOBankSettings.Uri, CKOBankDtotJson)).ReturnsAsync(_httpResponseMessageProviderMock.Object);

            //Configuring dependencies.
            harness = new InMemoryTestHarness();
            var services = new ServiceCollection();
            services.AddScoped(x => new PaymentConsumer(_httpClientProviderMock.Object, _paymentValidationServiceMock.Object, _ckoMapperMock.Object, _loggerMock.Object, Options.Create(_cKOBankSettings), Options.Create(_rabbitMQSettings)));
            services.BuildServiceProvider(true);
            var serviceProvider = services.BuildServiceProvider();
            _paymentConsumer = harness.Consumer(() =>
            {
                var consumer = serviceProvider.GetRequiredService<PaymentConsumer>();
                return consumer;
            });
        }

        [Test]
        public async Task Cssonsume_WhenSuccessful_RespondsWithAcceptance()
        {
            await harness.Start();
            try
            {
                await harness.InputQueueSendEndpoint.Send<IncomingPayment>(_paymentDetails);

                Assert.That(_paymentConsumer.Consumed.Select<IncomingPayment>().Any(), Is.True);
                _httpClientProviderMock.Verify(x => x.PostAsync(_cKOBankSettings.Uri, CKOBankDtotJson), Times.Once);
                _httpResponseMessageProviderMock.Verify(x => x.EnsureSuccessStatusCode(), Times.Once);
                Assert.That(harness.Sent.Select<IncomingPayment>().Any(), Is.True);
            }
            finally
            {
                await harness.Stop();
            }
        }
    }
}
