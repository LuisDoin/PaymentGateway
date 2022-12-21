using ServiceIntegrationLibrary.Utils.Interfaces;

namespace ServiceIntegrationLibrary.Utils
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

        public async Task<HttpResponseMessage> GetAsync(string? requestUri)
        {
            return await _httpClient.GetAsync(requestUri);
        }

        public async Task<IHttpResponseMessageProvider> PostAsync(string? requestUri, StringContent content)
        {
            return new HttpResponseMessageProvider(await _httpClient.PostAsync(requestUri, content));
        }
    }
}
