using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Framework
{
    /// <summary>
    /// Базовый класс для определения сервиса работы с HTTP запросами. Содержит возможности перехвата выполнения запросов, их построения и обработки ошибок
    /// </summary>
    public abstract class HttpService : IHttpService, IRequestObserver, IDisposable
    {
        /// <summary>
        /// Текуйщий обработчик запросов
        /// </summary>
        public HttpClientHandler Handler { get; }

        /// <summary>
        /// Текущий клиент работы с HTTP запросами
        /// </summary>
        public HttpClient Client { get; }

        /// <summary>
        /// Создает экземпляр сервиса, вызывает инициализацию HttpClientHandler и HttpClient. 
        /// Выполняет замещение функции работы с SSL в свойстве ServicePointManager.ServerCertificateValidationCallback
        /// </summary>
        public HttpService()
        {
            ConfigureHandler(Handler = CreateHandler());
            ConfigureClient(Client = new HttpClient(Handler, true));
            ServicePointManager.ServerCertificateValidationCallback = ValidateSertificate;
        }

        /// <summary>
        /// Метод, определяющий политику взаимодействия с SSL сертификатами X509. По умолчанию возвращает true для любого сертификата
        /// </summary>
        /// <param name="sender">Отправитель запроса</param>
        /// <param name="certificate">Сертификат</param>
        /// <param name="chain">Связка ключей</param>
        /// <param name="sslPolicyErrors">Ошибки политики SSL</param>
        /// <returns>Возвращает, является ли данный сертификат доверенным</returns>
        protected virtual bool ValidateSertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// Создает HttpClientHandler, который будет обрабатывать запросы. По умолчанию используется реализация из ModernHttpClient
        /// </summary>
        /// <returns></returns>
        protected virtual HttpClientHandler CreateHandler()
        {
            if (Bootstrapper.CurrentBatch.HasServiceEntry<IHttpClientHandlerProvider>())
                return Service<IHttpClientHandlerProvider>.Get().CreateHandler();
            return new HttpClientHandler();
        }

        /// <summary>
        /// Выполняет конфигурацию созданного HttpClientHandler
        /// </summary>
        /// <param name="handler">Ссылка на HttpClientHandler</param>
        protected abstract void ConfigureHandler(HttpClientHandler handler);

        /// <summary>
        /// Выполняет конфигурацию HttpClient
        /// </summary>
        /// <param name="client">Ссылка на HttpClient</param>
        protected abstract void ConfigureClient(HttpClient client);

        public RequestBuilder Build(string path = null, UriKind kind = UriKind.Relative)
        {
            var builder = new RequestBuilder(this, Client, path, kind);

            OnBuildingStarted(builder);

            return builder;
        }

        /// <summary>
        /// Вызывается сразу же в момент вызова метода Build при нового построении запроса
        /// </summary>
        /// <param name="builder">Ссылка на новый строитель запроса</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnBuildingStarted(RequestBuilder builder)
        {
        }

        void IRequestObserver.OnBeforeContinue<T>(RequestHandler<T> handler, T result) => OnBeforeContinue(handler, result);

        /// <summary>
        /// Вызывает перед каждым продолжением преобразования результата запроса
        /// </summary>
        /// <typeparam name="T">Тип объекта до преобразования</typeparam>
        /// <param name="handler">Обработчик результата запроса</param>
        /// <param name="result">Результат на данном этапе преобразований</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnBeforeContinue<T>(RequestHandler<T> handler, T result)
        {
            // Nothing
        }

        void IRequestObserver.OnAfterContinue<T, B>(RequestHandler<T> handler, B result) => OnAfterContinue(handler, result);
        
        /// <summary>
        /// Вызывает после каждого преобразования результата запроса
        /// </summary>
        /// <typeparam name="T">Тип объекта до преобразования</typeparam>
        /// <typeparam name="B">Тип объекта после преобразования</typeparam>
        /// <param name="handler">Обработчик результата запроса</param>
        /// <param name="result">Результат после преобразования</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnAfterContinue<T, B>(RequestHandler<T> handler, B result)
        {
            // Nothing
        }
        
        void IRequestObserver.OnRequestCreated(RequestBuilder builder) => OnRequestCreated(builder);

        /// <summary>
        /// Вызывается каждый раз, когда создан строитель запроса
        /// </summary>
        /// <param name="builder">Ссылка на строитель</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnRequestCreated(RequestBuilder builder)
        {
            // Nothing
        }

        void IRequestObserver.OnRequestStarted(RequestHandler<HttpResponseMessage> handler) => OnRequestStarted(handler);

        /// <summary>
        /// Вызывается каждый раз, когда начался новый запрос
        /// </summary>
        /// <param name="handler">Первый обработчик запроса</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnRequestStarted(RequestHandler<HttpResponseMessage> handler)
        {
            // Nothing
        }

        void IRequestObserver.OnRequestFinished(RequestHandler<HttpResponseMessage> handler) => OnRequestFinished(handler);

        /// <summary>
        /// Вызывается каждый раз, когда запрос был завершен
        /// </summary>
        /// <param name="handler">Первый обработчик запроса</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnRequestFinished(RequestHandler<HttpResponseMessage> handler)
        {
            // Nothing
        }

        void IRequestObserver.OnValidationFailed<T>(RequestHandler<T> handler, T result, Exception ex, Action repeateValidate) => OnValidationFailed(handler, result, ex, repeateValidate);

        /// <summary>
        /// Вызывается каждый раз, когда валидация результата выдала ошибку
        /// </summary>
        /// <typeparam name="T">Тип результата</typeparam>
        /// <param name="handler">Проверяемый обработчик запроса</param>
        /// <param name="result">Объект, который не прошел валидацию</param>
        /// <param name="ex">Исключение</param>
        /// <param name="repeateValidate">Делегат для повтора валидации</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void OnValidationFailed<T>(RequestHandler<T> handler, T result, Exception ex, Action repeateValidate)
        {
            // Nothing
        }

        string IRequestObserver.BuildHttpExceptionMessage(RequestBuilder builder, HttpResponseMessage res, HttpRequestException ex) => BuildHttpExceptionMessage(builder, res, ex);

        /// <summary>
        /// Выполняет построение текста сообщения исходя из результата запроса и вознившего исключения 
        /// </summary>
        /// <param name="builder">Строитель запроса</param>
        /// <param name="res">Сообщение - результат запроса</param>
        /// <param name="ex">Вознившее исключение</param>
        /// <returns>Возвращает текст ошибки, соответствующий данному запросу и исключению</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual string BuildHttpExceptionMessage(RequestBuilder builder, HttpResponseMessage res, HttpRequestException ex)
        {
            return ex.Message;
        }

        string IRequestObserver.BuildNoConnectionExceptionMessage(RequestBuilder builder, Exception ex) => BuildNoConnectionExceptionMessage(builder, ex);

        /// <summary>
        /// Выполняет построение текста сообщения исходя из нативного исключения во время установления соединения и/или отправки запроса
        /// </summary>
        /// <param name="builder">Строитель запроса</param>
        /// <param name="ex">Нативное исключение</param>
        /// <returns>Возвращает текст ошибки</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual string BuildNoConnectionExceptionMessage(RequestBuilder builder, Exception ex)
        {
            return "Отсутствует подключение к сети интернет";
        }

        Task<HttpResponseMessage> IRequestObserver.TransformSendTask(RequestBuilder builder, HttpRequestMessage message, Func<Task<HttpResponseMessage>> send) => TransformSendTask(builder, message, send);
        
        /// <summary>
        /// Выполняет преобразование задачи отправки HTTP сообщения перед обработкой результата по желанию наблюдателя.
        /// Обязан вызвать send функцию внутри тела метода
        /// </summary>
        /// <param name="builder">Строитель запроса</param>
        /// <param name="message">Отправленное сообщение</param>
        /// <param name="send">Функция отправки сообщения, которая должна быть вызвана в методе</param>
        /// <returns>Возвращает начатую задачу отправки сообщения</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual Task<HttpResponseMessage> TransformSendTask(RequestBuilder builder, HttpRequestMessage message, Func<Task<HttpResponseMessage>> send)
        {
            return send();
        }

        #region IDisposable Support
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Client?.Dispose();
                }
                _disposed = true;
            }
        }
        
         ~HttpService()
         {
            Dispose(false);
         }
        
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
