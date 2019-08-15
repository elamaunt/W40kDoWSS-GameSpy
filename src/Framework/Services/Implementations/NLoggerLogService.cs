using NLog;
using System;
using System.Diagnostics;

namespace Framework
{
    public class NLoggerLogService : ILogService
    {
        public void Write(object obj, StackFrame frame, LogLevel logLevel = null)
        {
            var logger = LogManager.GetLogger(frame.GetMethod().DeclaringType.FullName);

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
