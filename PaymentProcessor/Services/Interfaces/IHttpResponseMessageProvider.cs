namespace PaymentProcessor.Services.Interfaces
{
    public interface IHttpResponseMessageProvider : IDisposable
    {
        public HttpResponseMessage EnsureSuccessStatusCode();
    }
}
