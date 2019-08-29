using System;
using System.Threading;
using System.Threading.Tasks;

namespace Framework
{
    /// <summary>
    /// Класс, инкапсулирующий некий промежуточный результат обработки HTTP запроса
    /// </summary>
    /// <typeparam name="T">Тип результата</typeparam>
    public sealed class RequestHandler<T>
    {
        internal readonly IRequestObserver Observer;
        private readonly CancellationToken _token;

        /// <summary>
        /// Строитель данного запроса
        /// </summary>
        public readonly RequestBuilder Builder;

        /// <summary>
        /// Задача данного обработчика
        /// </summary>
        public readonly Task<T> Task;
       
        /// <summary>
        /// Создает обработчик запроса
        /// </summary>
        /// <param name="builder">Строитель запроса</param>
        /// <param name="observer">Наблюдатель запроса</param>
        /// <param name="task">Задача запроса</param>
        public RequestHandler(RequestBuilder builder, IRequestObserver observer, Task<T> task)
        {
            Builder = builder;
            Observer = observer;
            Task = task;
        }

        /// <summary>
        /// Выполняет конвертацию результата в другой тип результата при помощи указанного делегата 
        /// </summary>
        /// <typeparam name="B">Требуемый тип результата</typeparam>
        /// <param name="converter">Делегат, конвертирующий результат</param>
        /// <returns>Возвращает преобразованный обработчик запроса</returns>
        public RequestHandler<B> Continue<B>(Converter<T, B> converter)
        {
            return new RequestHandler<B>(Builder, Observer, Task.ContinueWith(task => Convert(task, converter)));
        }

        /// <summary>
        /// Выполняет конвертацию результата в другой тип результата при помощи указанного делегата в виде асинхронной задачи
        /// </summary>
        /// <typeparam name="B">Требуемый тип результата</typeparam>
        /// <param name="converter">Делегат, конвертирующий результат</param>
        /// <returns>Возвращает преобразованный обработчик запроса</returns>
        public RequestHandler<B> Continue<B>(Converter<T, Task<B>> converter)
        {
            return new RequestHandler<B>(Builder, Observer, Task.ContinueWith(task => Convert(task, converter)).Unwrap());
        }

        /// <summary>
        /// Выполняет валидацию результата на текущем шаге преобразований при помощи указанного делегата.
        /// </summary>
        /// <param name="validator">Делегат, выполняющий проверку</param>
        /// <returns>Возвращает ссылку на обработчик результата запроса после проверки</returns>
        public RequestHandler<T> Validate(Action<T> validator)
        {
            var validation = Task.ContinueWith(task =>
            {
                _token.ThrowIfCancellationRequested();
                var result = task.Result;

                try
                {
                    validator(result);
                }
                catch (Exception ex)
                {
                    Observer.OnValidationFailed(this, result, ex, () => validator(result));
                    throw;
                }
                return result;
            });

            return new RequestHandler<T>(Builder, Observer, validation);
        }

        private B Convert<B>(Task<T> task, Converter<T, B> converter)
        {
            _token.ThrowIfCancellationRequested();
            var result = task.Result;
            Observer.OnBeforeContinue(this, result);
            var convertedResult = converter(result);
            Observer.OnAfterContinue(this, convertedResult);
            return convertedResult;
        }

        /// <summary>
        /// Извлекает задачу из данного обработчика запроса
        /// </summary>
        /// <param name="self">Ссылка на обработчик запроса</param>
        public static implicit operator Task<T>(RequestHandler<T> self)
        {
            return self.Task;
        }
    }
}
