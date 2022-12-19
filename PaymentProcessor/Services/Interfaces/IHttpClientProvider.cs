namespace PaymentProcessor.Services.Interfaces
{
    public interface IHttpClientProvider
    {
        public Task<IHttpResponseMessageProvider> PostAsync(string? requestUri, string content);
    }
}
