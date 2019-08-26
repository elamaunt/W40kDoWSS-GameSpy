using System.Net.Http;

namespace Framework
{
    public interface IHttpClientHandlerProvider
    {
        HttpClientHandler CreateHandler();
    }
}
