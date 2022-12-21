using ServiceIntegrationLibrary.Utils;
using System.Text.Json.Serialization;

namespace ServiceIntegrationLibrary.Models
{
    public class ProcessedPayment
    {
        public ProcessedPayment()
        {
        }

        public ProcessedPayment(IncomingPayment IncomingPayment)
        {
            this.IncomingPayment = IncomingPayment;
            ProcessedAt = DateTime.UtcNow;
        }

        public IncomingPayment IncomingPayment { get; set; }

        public DateTime ProcessedAt { get; set; }
    }
}
