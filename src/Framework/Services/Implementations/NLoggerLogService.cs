using NLog;
using System;
using System.Diagnostics;

namespace Framework
{
    public class NLoggerLogService : ILogService
    {
        public void Write(object obj, StackFrame frame)
        {
            var logger = LogManager.GetLogger(frame.GetMethod().DeclaringType.FullName);
            var exception = obj as Exception;

            if (exception != null)
                logger.Error(exception);
            else
                logger.Log(LogLevel.Info, obj);
        }
    }
}
