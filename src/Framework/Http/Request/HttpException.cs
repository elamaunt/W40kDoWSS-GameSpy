using System;
using System.Net.Http;

namespace Framework
{
    public class HttpException : Exception
    {
        public HttpRequestException RequestException { get; private set; }

        public HttpResponseMessage ResponceMessage { get; private set; }

        public RequestBuilder RequestBuilder { get; private set; }
        public HttpException(string message, RequestBuilder builder, HttpResponseMessage responceMessage, HttpRequestException ex)
            : base(message)
        {
            RequestBuilder = builder;
            RequestException = ex;
            ResponceMessage = responceMessage;
        }
    }
}
