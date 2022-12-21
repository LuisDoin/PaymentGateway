
using System.Threading.Tasks;

namespace ServiceIntegrationLibrary.Utils.Interfaces
{
    public interface IHttpClientProvider
    {
        Task<IHttpResponseMessageProvider> PostAsync(string? requestUri, string content);
        Task<HttpResponseMessage> GetAsync(string? requestUri);
    }
}
