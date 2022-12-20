using PaymentProcessor.Services.Interfaces;

namespace PaymentProcessor.Services
{
    //This class provide us greater freedom during testing.
    public class HttpResponseMessageProvider : IHttpResponseMessageProvider
    {
        public readonly HttpResponseMessage _httpResponseMessage;

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
