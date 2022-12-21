namespace TransactionsApi.Config
{
    public class RabbitMQSettings
    {
        public string Uri { get; set; }
        public string SuccessfulTransactionsQueue { get; set; }
        public string UnuccessfulTransactionsQueue { get; set; }
    }
}
