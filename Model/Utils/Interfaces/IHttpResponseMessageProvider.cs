using System;
using System.Net.Http;

namespace ServiceIntegrationLibrary.Utils.Interfaces
{
    public interface IHttpResponseMessageProvider : IDisposable
    {
        HttpResponseMessage EnsureSuccessStatusCode();
    }
}
