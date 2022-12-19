namespace PaymentProcessor.Config
{
    public class RabbitMQSettings
    {
        public string pendingTransactionsQueue { get; set; }
        public string Uri { get; set; }
    }
}
