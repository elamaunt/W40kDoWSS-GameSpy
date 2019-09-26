using NLog;
using System;
using System.Diagnostics;
using System.IO;

namespace Framework
{
    public class NLoggerLogService : ILogService
    {
        public void Write(object obj, LogLevel logLevel, string callerFilePath, string callerMemberName, int sourceLineNumber)
        {
            var name = $"{Path.GetFileName(callerFilePath)} line {sourceLineNumber} / {callerMemberName}";
            var logger = LogManager.GetLogger(name);

            if (Debugger.IsAttached)
                Console.WriteLine(obj.ToString());

            logger.Log(logLevel, obj);
        }
    }
}
