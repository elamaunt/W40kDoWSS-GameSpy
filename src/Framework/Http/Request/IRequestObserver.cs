using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Framework
{
    public interface IRequestObserver
    {
        string BuildHttpExceptionMessage(RequestBuilder builder, HttpResponseMessage res, HttpRequestException ex);

        string BuildNoConnectionExceptionMessage(RequestBuilder builder, Exception ex);

        void OnBeforeContinue<T>(RequestHandler<T> handler, T result);

        void OnAfterContinue<T, B>(RequestHandler<T> handler, B result);

        void OnRequestCreated(RequestBuilder builder);

        void OnRequestStarted(RequestHandler<HttpResponseMessage> handler);

        void OnRequestFinished(RequestHandler<HttpResponseMessage> handler);

        void OnValidationFailed<T>(RequestHandler<T> handler, T result, Exception ex, Action repeateValidate);

        Task<HttpResponseMessage> TransformSendTask(RequestBuilder builder, HttpRequestMessage message, Func<Task<HttpResponseMessage>> send);
    }
}
