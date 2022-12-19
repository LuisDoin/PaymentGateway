using PaymentProcessor.Services.Interfaces;

namespace PaymentProcessor.Services
{
    public class HttpResponseMessageProvider : IHttpResponseMessageProvider
    {
        private readonly HttpResponseMessage _httpResponseMessage;

        public HttpResponseMessageProvider(HttpResponseMessage httpResponseMessage)
        {
            _httpResponseMessage = httpResponseMessage;
        }

        public HttpResponseMessage EnsureSuccessStatusCode()
        {
            return _httpResponseMessage.EnsureSuccessStatusCode();
        }

        public void Dispose()
        {
            _httpResponseMessage.Dispose();
        }
    }
}
