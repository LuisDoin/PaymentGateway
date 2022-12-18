namespace PaymentGateway.Services.Interfaces
{
    public interface IQueueProducer
    {
        public void Publish(string message);
    }
}
