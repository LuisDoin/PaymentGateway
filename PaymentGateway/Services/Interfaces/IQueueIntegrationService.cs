namespace PaymentGateway.Services.Interfaces
{
    public interface IQueueIntegrationService
    {
        public void Publish(string message);
    }
}
