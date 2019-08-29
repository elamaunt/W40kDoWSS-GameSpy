using System;
using System.Net.Http;

namespace Framework
{
   
    public interface IHttpService
    {
        HttpClientHandler Handler { get; }

        HttpClient Client { get; }

        RequestBuilder Build(string path = null, UriKind kind = UriKind.Relative);
    }
}
