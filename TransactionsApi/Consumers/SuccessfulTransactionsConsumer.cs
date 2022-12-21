using MassTransit;
using ServiceIntegrationLibrary.Models;
using TransactionsApi.Services;

namespace TransactionsApi.Consumers
{
    public class SuccessfulTransactionsConsumer : IConsumer<IncomingPayment>
    {
        private readonly ILogger<SuccessfulTransactionsConsumer> _logger;
        private readonly IPaymentServices _paymentServices;

        public SuccessfulTransactionsConsumer(ILogger<SuccessfulTransactionsConsumer> logger, IPaymentServices paymentServices)
        {
            _logger = logger;
            _paymentServices = paymentServices;
        }
        public async Task Consume(ConsumeContext<IncomingPayment> context)
        {
            try
            {
                _logger.LogInformation($"Processing successful payment {context.Message.PaymentId}");

                await _paymentServices.ProcessCompletedTransaction(context.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while processing successful payment {context.Message.PaymentId}.");
                throw;
            }
        }
    }
}
