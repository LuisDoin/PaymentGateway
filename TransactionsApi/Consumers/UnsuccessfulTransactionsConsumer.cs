using MassTransit;
using ServiceIntegrationLibrary.Models;
using ServiceIntegrationLibrary.Utils;
using TransactionsApi.Services;

namespace TransactionsApi.Consumers
{
    public class UnsuccessfulTransactionsConsumer : IConsumer<Fault<PaymentDetails>>
    {
        private readonly ILogger<UnsuccessfulTransactionsConsumer> _logger;
        private readonly IPaymentServices _paymentServices;

        public UnsuccessfulTransactionsConsumer(ILogger<UnsuccessfulTransactionsConsumer> logger, IPaymentServices paymentServices)
        {
            _logger = logger;
            _paymentServices = paymentServices;
        }
        public async Task Consume(ConsumeContext<Fault<PaymentDetails>> context)
        {
            try
            {
                var paymentDetails = context.Message.Message;
                paymentDetails.Status = PaymentStatus.Unsuccessful;
                _logger.LogInformation($"Processing unsuccessful payment {paymentDetails.PaymentId}");

                await _paymentServices.ProcessCompletedTransaction(paymentDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while processing unsuccessful payment {context.Message.Message.PaymentId}.");
                throw;
            }
        }
    }
}
