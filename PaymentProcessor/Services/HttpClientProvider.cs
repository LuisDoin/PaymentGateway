using PaymentProcessor.Services.Interfaces;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace PaymentProcessor.Services
{
    //This class provide us greater freedom during testing.
    public class HttpClientProvider : IHttpClientProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;

        public HttpClientProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _httpClient = _httpClientFactory.CreateClient();
        }

        public async Task<IHttpResponseMessageProvider> PostAsync(string? requestUri, string content)
        {
            return new HttpResponseMessageProvider(await _httpClient.PostAsync(requestUri, new StringContent(content, Encoding.UTF8, Application.Json)));
        }
    }
}
