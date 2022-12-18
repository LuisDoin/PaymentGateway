using MassTransit;
using Model;

namespace PaymentProcessor.Consumers
{
    public class PaymentConsumer : IConsumer<PaymentDetails>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<PaymentConsumer> _logger;

        public PaymentConsumer(IPublishEndpoint publishEndpoint, ILogger<PaymentConsumer> logger, )
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public Task Consume(ConsumeContext<PaymentDetails> context)
        {
            try
            {
                var message = context.Message;
                var paymentId = message.PaymentId;

                _logger.LogInformation("Processing payment:" + paymentId);



                _publishEndpoint.Publish(context.Message);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while processing message " + context.Message + " - Error: " + ex);
                throw;
            }
            
        }
    }
}
