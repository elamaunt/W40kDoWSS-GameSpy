using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;

namespace Framework
{
    /// <summary>
    /// Тип делегата обработки прогресса загрузки файла
    /// </summary>
    /// <param name="totalBytes"></param>
    /// <param name="readedBytes"></param>
    /// <param name="percentage"></param>
    public delegate void ProgressHandler(long totalBytes, long readedBytes, float percentage);
    public static class RequestHandlerExtentionMethods
    {
        /// <summary>
        /// Добавляет валидацию результата Http запроса на код 200. Если код отличается, то будет брошено исключение
        /// </summary>
        /// <param name="self">Ссылка на текущий обработчик</param>
        /// <returns>Возвращает преобразованный обработчик результата запроса</returns>
        public static RequestHandler<HttpResponseMessage> ValidateSuccessStatusCode(this RequestHandler<HttpResponseMessage> self)
        {
            return self.Validate(res =>
            {
                try
                {
                    res.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException ex)
                {
                    throw new HttpException(self.Observer.BuildHttpExceptionMessage(self.Builder, res, ex), self.Builder, res, ex);
                }
            });
        }

        /// <summary>
        /// Создает обработчик резльутата с промежуточным преобразованием запроса в Json
        /// </summary>
        /// <typeparam name="B">Требуемый тип</typeparam>
        /// <param name="self">Ссылка на текущий обработчик запроса</param>
        /// <returns>Возвращает преобразованный обработчик результата запроса</returns>
        public static RequestHandler<B> Json<B>(this RequestHandler<string> self)
        {
            return self.Continue(JsonConvert.DeserializeObject<B>);
        }

        /// <summary>
        /// Создает обработчик резльутата с промежуточным преобразованием результата запроса в Json
        /// </summary>
        /// <typeparam name="B">Требуемый тип</typeparam>
        /// <param name="self">Ссылка на текущий обработчик запроса</param>
        /// <param name="serializer">Настройка сериализации</param>
        /// <returns>Возвращает преобразованный обработчик результата запроса</returns>
        public static RequestHandler<B> Json<B>(this RequestHandler<HttpResponseMessage> self, JsonSerializer serializer)
        {
            return self.Continue(mes => mes.Content.ReadAsStreamAsync())
                .Continue(stream =>
                {
                    using (var sr = new StreamReader(stream))
                    {
                        using (var jsonTextReader = new JsonTextReader(sr))
                        {
                            return serializer.Deserialize<B>(jsonTextReader);
                        }
                    }
                });
        }

        /// <summary>
        /// Создает обработчик резльутата с промежуточным извлеченим контента из ответного сообщения
        /// </summary>
        /// <param name="self">Ссылка на текущий обработчик запроса</param>
        /// <returns>Возвращает преобразованный обработчик результата запроса</returns>
        public static RequestHandler<HttpContent> Content(this RequestHandler<HttpResponseMessage> self)
        {
            return self.Continue(mes => mes.Content);
        }

        /// <summary>
        /// Создает обработчик резльутата с промежуточным извлеченим контента в виде строки из ответного сообщения
        /// </summary>
        /// <param name="self">Ссылка на текущий обработчик запроса</param>
        /// <returns>Возвращает преобразованный обработчик результата запроса</returns>
        public static RequestHandler<string> String(this RequestHandler<HttpContent> self)
        {
            return self.Continue(content => content.ReadAsStringAsync());
        }

        /// <summary>
        /// Создает обработчик резльутата с промежуточным извлеченим контента в виде потока данных из ответного сообщения
        /// </summary>
        /// <param name="self">Ссылка на текущий обработчик запроса</param>
        /// <returns>Возвращает преобразованный обработчик результата запроса</returns>
        public static RequestHandler<Stream> Stream(this RequestHandler<HttpContent> self)
        {
            return self.Continue(content => content.ReadAsStreamAsync());
        }

        /// <summary>
        /// Создает обработчик резльутата с промежуточным извлеченим контента в виде массива байт из ответного сообщения
        /// </summary>
        /// <param name="self">Ссылка на текущий обработчик запроса</param>
        /// <returns>Возвращает преобразованный обработчик результата запроса</returns>
        public static RequestHandler<byte[]> ByteArray(this RequestHandler<HttpContent> self)
        {
            return self.Continue(content => content.ReadAsByteArrayAsync());
        }

        /// <summary>
        /// Создает обработчик резульата, который выполнит промежуточную загрузку контента запроса в указанный поток данных.
        /// Позволяет отслеживать загрузку файла
        /// </summary>
        /// <param name="self">Ссылка на текущий обработчик запроса</param>
        /// <param name="token">Токен отмены операции</param>
        /// <param name="destinationStream">Целевой поток для сохранения файла контента</param>
        /// <param name="bufferSize">Размер буфера при загрзуке</param>
        /// <param name="handler">Делегат - обработчик загрузки</param>
        /// <param name="readsOfOneUpdate">Чтений потока контента перед вызовом делегата обновления</param>
        /// <returns>Возвращает преобразованный обработчик результата запроса</returns>
        public static RequestHandler<Stream> ProcessLoading(this RequestHandler<HttpContent> self, CancellationToken token, Stream destinationStream, long bufferSize, ProgressHandler handler, int readsOfOneUpdate = 100)
        {
            return self.Continue(async content =>
            {
                token.ThrowIfCancellationRequested();

                var stream = await content.ReadAsStreamAsync();

                var totalDownloadSize = content.Headers.ContentLength;
                var totalBytesRead = 0L;
                var readCount = 0L;
                var buffer = new byte[bufferSize];
                var isMoreToRead = true;

                do
                {
                    token.ThrowIfCancellationRequested();

                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        isMoreToRead = false;
                        handler(totalDownloadSize.GetValueOrDefault(), totalBytesRead, 1f);
                        continue;
                    }

                    await destinationStream.WriteAsync(buffer, 0, bytesRead);

                    totalBytesRead += bytesRead;
                    readCount += 1;

                    if (readCount % readsOfOneUpdate == 0)
                    {
                        var sizeValue = totalDownloadSize.GetValueOrDefault();

                        if (sizeValue == 0)
                            handler(sizeValue, totalBytesRead, 0f);
                        else
                            handler(sizeValue, totalBytesRead, (float)Math.Round((double)totalBytesRead / sizeValue, 2));
                    }
                }
                while (isMoreToRead);

                token.ThrowIfCancellationRequested();

                return destinationStream;
            });
        }
    }
}
