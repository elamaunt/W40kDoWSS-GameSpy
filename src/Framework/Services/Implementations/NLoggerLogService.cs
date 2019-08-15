using NLog;
using System;
using System.IO;

namespace Framework
{
    public class NLoggerLogService : ILogService
    {
        public void Write(object obj, string callerFilePath, string callerMemberName, int sourceLineNumber)
        {
            var name = $"{Path.GetFileName(callerFilePath)} / {callerMemberName} / {sourceLineNumber}";
            var logger = LogManager.GetLogger(name);
            var exception = obj as Exception;

            if (exception != null)
                logger.Error(exception);
            else
                logger.Log(LogLevel.Info, obj);
        }
    }
}
