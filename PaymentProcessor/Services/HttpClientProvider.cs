using PaymentProcessor.Config;
using PaymentProcessor.Services.Interfaces;
using System.Net.Http;
using static System.Net.Mime.MediaTypeNames;
using System.Text;

namespace PaymentProcessor.Services
{
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
