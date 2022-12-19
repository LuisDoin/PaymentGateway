namespace PaymentProcessor.Config
{
    public class RabbitMQSettings
    {
        public string Uri { get; set; }
        public string PendingTransactionsQueue { get; set; }
        public string CompletedTransactionsQueue { get; set; }
    }
}
