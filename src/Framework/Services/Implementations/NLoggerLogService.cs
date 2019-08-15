using NLog;
using System;
using System.IO;

namespace Framework
{
    public class NLoggerLogService : ILogService
    {
        public void Write(object obj, LogLevel logLevel, string callerFilePath, string callerMemberName, int sourceLineNumber)
        {
            var name = $"{Path.GetFileName(callerFilePath)} / {callerMemberName} / {sourceLineNumber}";
            var logger = LogManager.GetLogger(name);

            if (logLevel == null)
            {
                if (obj is Exception)
                    logLevel = LogLevel.Error;
                else
                    logLevel = LogLevel.Info;
            }

            logger.Log(logLevel, obj);
        }
    }
}
