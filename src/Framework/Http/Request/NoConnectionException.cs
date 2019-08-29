using System;

namespace Framework
{
    /// <summary>
    /// Вспомогательное исключение для перехвата ошибок отправки и/или установления соединения.
    /// Содержит в себе внутреннее исключение, но не в поле InnerException, а в свойстве NativeException
    /// </summary>
    public class NoConnectionException : Exception
    {
        public Exception NativeException { get; }

        public NoConnectionException(string message, Exception nativeException)
            : base(message)
        {
            NativeException = nativeException;
        }
    }
}
