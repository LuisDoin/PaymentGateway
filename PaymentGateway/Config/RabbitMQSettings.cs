namespace PaymentProcessor.Config
{
    public class RabbitMQSettings
    {
        public string PendingTransactionsQueue { get; set; }
        public string Uri { get; set; }
    }
}
