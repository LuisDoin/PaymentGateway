namespace TransactionsApi.Config
{
    public class RabbitMQSettings
    {
        public string Uri { get; set; }
        public string CompletedTransactionsQueue { get; set; }
    }
}
