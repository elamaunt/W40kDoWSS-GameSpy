using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Framework
{
    /// <summary>
    /// Класс - строитель Http запроса
    /// </summary>
    public sealed class RequestBuilder
    {
        private readonly HttpClient _client;

        private readonly Uri _uri;
        private readonly NameValueCollection _keyValue;
        private string _additionalQuery;

        /// <summary>
        /// Построенное Http сообщение
        /// </summary>
        public HttpRequestMessage Message { get; }

        /// <summary>
        /// Идентификатор данного запроса (получен икрементом)
        /// </summary>
        public int RequestId { get; private set; }
        private static volatile int _requestsCounter;
        private static volatile int _currentRequestsCounter;

        /// <summary>
        /// Количество запросов, которое было произведено
        /// </summary>
        public int RequestsCounter => _requestsCounter;

        /// <summary>
        /// Количество активных запросов в данный момент
        /// </summary>
        public int CurrentRequestsCounter => _currentRequestsCounter;

        /// <summary>
        /// Окончательное Uri запроса
        /// </summary>
        public Uri Uri
        {
            get
            {
                UriBuilder builder;
                builder = new UriBuilder(_uri);

                if (_additionalQuery == null)
                    builder.Query = _keyValue.ToString();
                else
                {
                    var q = _keyValue.ToString();

                    if (q.Length == 0)
                        builder.Query = _additionalQuery;
                    else
                        builder.Query = q + "&" + _additionalQuery;
                }

                if (_additionalQuery == null)
                    return builder.Uri;
                else
                    return builder.Uri;
            }
        }

        /// <summary>
        /// Определяет, определен ли параметр с указанным ключом
        /// </summary>
        /// <param name="name">Имя ключа</param>
        /// <returns>Флаг о наличии параметра</returns>
        public bool HasParamater(string name) => !_keyValue.Get(name).IsNullOrWhiteSpace();

        /// <summary>
        /// Определяет, определен ли заголовок запроса с указанным ключом
        /// </summary>
        /// <param name="name">Имя ключа</param>
        /// <returns>Флаг о наличии заголовка</returns>
        public bool HasHeader(string name) => Message.Headers.Contains(name);

        /// <summary>
        /// Возвращает наличие дополнительной строки запроса для Uri
        /// </summary>
        public bool HasAdditionalQuery => !_additionalQuery.IsNullOrWhiteSpace();

        /// <summary>
        /// Окончательное Uri запроса, преобразованное в строку
        /// </summary>
        public string UriString => Uri.ToString();

        internal readonly IRequestObserver Observer;

        private List<Action<RequestBuilder>> _beforeSend;
        private List<Action<RequestBuilder>> _afterSend;

        internal RequestBuilder(IRequestObserver observer, HttpClient client, string path, UriKind kind = UriKind.Relative)
        {
            Observer = observer;
            _client = client;

            var uri = new Uri(path, kind);

            if (!uri.IsAbsoluteUri)
                uri = new Uri(client.BaseAddress, uri);

            _keyValue = HttpUtility.ParseQueryString(uri.Query);
            _uri = new Uri(uri.GetLeftPart(UriPartial.Path));

            Message = new HttpRequestMessage();
        }

        /// <summary>
        /// Добавляет делегат, который будет вызван сразу после отправки запроса
        /// </summary>
        /// <param name="handler">Ссылка на делегат</param>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder AfterSend(Action<RequestBuilder> handler)
        {
            if (_afterSend == null)
                _afterSend = new List<Action<RequestBuilder>>();

            _afterSend.Add(handler);
            return this;
        }

        /// <summary>
        /// Добавляет делегат, который будет непосредственно до отправки запроса
        /// </summary>
        /// <param name="handler">Ссылка на делегат</param>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder BeforeSend(Action<RequestBuilder> handler)
        {
            if (_beforeSend == null)
                _beforeSend = new List<Action<RequestBuilder>>();

            _beforeSend.Add(handler);
            return this;
        }

        /// <summary>
        /// Добавляет пользовательскую строку в Uri запроса
        /// </summary>
        /// <param name="query">Ссылка на строку</param>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder WithAdditionalQuery(string query)
        {
            _additionalQuery = query;
            return this;
        }

        /// <summary>
        /// Добавляет указанные параметры в строку запроса
        /// </summary>
        /// <typeparam name="T">Тип объектов, которые будут преобразованы в строку</typeparam>
        /// <param name="dict">Сроварь параметров</param>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder WithParameters<T>(Dictionary<string, T> dict)
          where T : struct
        {
            if (ReferenceEquals(null, dict))
                return this;

            foreach (var item in dict)
                _keyValue.Set(item.Key, item.Value.ToString());

            return this;
        }

        /// <summary>
        /// Добавляет указанные параметры в строку запроса
        /// </summary>
        /// <param name="dict">Сроварь параметров</param>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder WithParameters(Dictionary<string, string> dict)
        {
            if (ReferenceEquals(null, dict))
                return this;

            foreach (var item in dict)
                _keyValue.Set(item.Key, item.Value);

            return this;
        }

        /// <summary>
        /// Добавляет новый параметр в строку запроса или заменяет текущий по указанному ключу
        /// </summary>
        /// <typeparam name="T">Тип параметра, котроый будет преобразован в строку</typeparam>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder WithParameter<T>(string key, T value)
            where T : struct
        {
            _keyValue.Set(key, value.ToString());
            return this;
        }

        /// <summary>
        /// Добавляет новый параметр в строку запроса или заменяет текущий по указанному ключу
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder WithParameter(string key, string value)
        {
            if (ReferenceEquals(null, value))
                return this;
            _keyValue.Set(key, value);
            return this;
        }

        /// <summary>
        /// Добавляет новое свойство запроса или заменяет текущее по указанному ключу
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder WithProperty(string key, object value)
        {
            Message.Properties.Add(key, value);
            return this;
        }

        /// <summary>
        /// Добавляет контент в запрос
        /// </summary>
        /// <param name="content">Ссылка на контент</param>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder WithContent(HttpContent content)
        {
            Message.Content = content;
            return this;
        }

        /// <summary>
        /// Добавляет объект, преобразуя его в Json, согласно указаннмы настройкам сериализации и кодировки
        /// </summary>
        /// <param name="content">Ссылка на контент</param>
        /// <param name="jsonSerializerSettings">Настройки сериализации</param>
        /// <param name="encoding">Кодировка, по умолчанию UTF-8</param>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder WithJsonContent(object content, JsonSerializerSettings jsonSerializerSettings, Encoding encoding = null)
        {
            var stringContent = JsonConvert.SerializeObject(content, jsonSerializerSettings);
            Message.Content = new StringContent(stringContent, encoding ?? Encoding.UTF8, "application/json");
            return this;
        }

        /// <summary>
        /// Добавляет Form-Data контент в запрос используя указанный массив байт и имена контента и файла
        /// </summary>
        /// <param name="content"></param>
        /// <param name="name"></param>
        /// <param name="filename"></param>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder WithFormDataContent(byte[] content, string name, string filename)
        {
            var requestContent = new MultipartFormDataContent();
            var byteContent = new ByteArrayContent(content);
            var splited = filename.Split('.');
            var contentType = MimeTypeMap.GetMimeType(splited.LastOrDefault());
            byteContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            byteContent.Headers.ContentLength = content.LongLength;
            requestContent.Add(byteContent, $"\"{name}\"", filename);
            Message.Content = requestContent;
            return this;
        }

        /// <summary>
        /// Добавляет новый заголовок запроса или заменяте текущий по казанному ключу
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder WithHeader(string key, string value)
        {
            if (Message.Headers.Contains(key))
                Message.Headers.Remove(key);

            Message.Headers.Add(key, value);
            return this;
        }

        /// <summary>
        /// Добавляет новый заголовок запроса, если он еще не был добавлен
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder AddHeaderIfNotAdded(string key, string value)
        {
            if (Message.Headers.Contains(key))
                return this;

            Message.Headers.Add(key, value);
            return this;
        }

        /// <summary>
        /// Добавляет новый заголовок запроса, если он еще не был добавлен
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="values">Значения</param>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder WithHeader(string key, IEnumerable<string> values)
        {
            Message.Headers.Add(key, values);
            return this;
        }

        /// <summary>
        /// Добавляет новый заголовок запроса без выполнения проверок
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder WithHeaderWithoutValidation(string key, string value)
        {
            Message.Headers.TryAddWithoutValidation(key, value);
            return this;
        }

        /// <summary>
        /// Добавляет новый заголовок запроса без выполнения проверок
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значения</param>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder WithHeaderWithoutValidation(string key, IEnumerable<string> values)
        {
            Message.Headers.TryAddWithoutValidation(key, values);
            return this;
        }

        /// <summary>
        /// Устанавливает тип запроса как POST
        /// </summary>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder Post()
        {
            Message.Method = HttpMethod.Post;
            return this;
        }

        /// <summary>
        /// Устанавливает тип запроса как GET
        /// </summary>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder Get()
        {
            Message.Method = HttpMethod.Get;
            return this;
        }

        /// <summary>
        /// Устанавливает тип запроса как PUT
        /// </summary>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder Put()
        {
            Message.Method = HttpMethod.Put;
            return this;
        }

        /// <summary>
        /// Устанавливает тип запроса как TRACE
        /// </summary>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder Trace()
        {
            Message.Method = HttpMethod.Trace;
            return this;
        }

        /// <summary>
        /// Устанавливает тип запроса как DELETE
        /// </summary>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder Delete()
        {
            Message.Method = HttpMethod.Delete;
            return this;
        }

        /// <summary>
        /// Устанавливает тип запроса как HEAD
        /// </summary>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder Head()
        {
            Message.Method = HttpMethod.Head;
            return this;
        }

        /// <summary>
        /// Устанавливает тип запроса как OPTIONS
        /// </summary>
        /// <returns>Возвращает строитель запроса</returns>
        public RequestBuilder Options()
        {
            Message.Method = HttpMethod.Options;
            return this;
        }

        /// <summary>
        /// Отправляет запрос, построенный текущим строителем
        /// </summary>
        /// <param name="option">Опции обработки запроса</param>
        /// <returns>Возвращает первый обработчик результата</returns>
        public RequestHandler<HttpResponseMessage> Send(HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
        {
            return Send(CancellationToken.None, option);
        }

        /// <summary>
        /// Отправляет запрос, построенный текущим строителем
        /// </summary>
        /// <param name="token">Токен отмены запроса</param>
        /// <param name="option">Опции обработки запроса</param>
        /// <returns>Возвращает первый обработчик результата</returns>
        public RequestHandler<HttpResponseMessage> Send(CancellationToken token, HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
        {
            Message.RequestUri = Uri;

            Observer.OnRequestCreated(this);

            if (!_beforeSend.IsNullOrEmpty())
                for (int i = 0; i < _beforeSend.Count; i++)
                    _beforeSend[i](this);

            RequestId = _requestsCounter++;
            _currentRequestsCounter++;

            var handler = new RequestHandler<HttpResponseMessage>(this, Observer, Observer.TransformSendTask(this, Message, () => SendRequest(token, option).WithBlockingCancellation(token)));

            Observer.OnRequestStarted(handler);

            if (!_afterSend.IsNullOrEmpty())
                for (int i = 0; i < _afterSend.Count; i++)
                    _afterSend[i](this);

            handler.Task.ContinueWith(t =>
            {
                _currentRequestsCounter--;
                Observer.OnRequestFinished(handler);
            }).NoWarning();

            return handler;
        }

        private async Task<HttpResponseMessage> SendRequest(CancellationToken token, HttpCompletionOption option)
        {
            try
            {
                if (Message.Method == HttpMethod.Post)
                {
                    var content = Message.Content ?? new StringContent("");

                    foreach (var item in Message.Headers)
                    {
                        content.Headers.Remove(item.Key);
                        content.Headers.Add(item.Key, item.Value);
                    }

                    return await _client.PostAsync(Message.RequestUri, content, token).ConfigureAwait(false);
                }
                return await _client.SendAsync(Message, option, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new NoConnectionException(Observer.BuildNoConnectionExceptionMessage(this, ex), ex);
            }
        }
    }
}

