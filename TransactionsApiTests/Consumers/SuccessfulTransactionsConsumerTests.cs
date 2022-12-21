using MassTransit.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TransactionsApi.Consumers;

namespace TransactionsApiTests.Consumers
{
    [TestFixture]
    class SuccessfulTransactionsConsumerTests
    {
        private InMemoryTestHarness harness;
        private Mock<ILogger<SuccessfulTransactionsConsumer> _loggerMock;
        private IConsumerTestHarness<SuccessfulTransactionsConsumer> _paymentConsumer;
        private IncomingPayment _paymentDetails;
        private CKOPaymentInfoDTO _ckoPaymentInfoDTO;
        private string CKOBankDtotJson;
    }
}
