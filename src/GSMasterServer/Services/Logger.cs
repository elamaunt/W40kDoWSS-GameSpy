using System;
using System.IO;
using System.Runtime.CompilerServices;
using NLog;

namespace GSMasterServer.Services
{
    public static class Logger
    {
        //LogLevel: 6
        public static void Fatal(object obj,
            [CallerFilePath]string callerFilePath = "",
            [CallerMemberName]string callerMemberName = "",
            [CallerLineNumber]int sourceLineNumber = 0)
        {
            Write(obj, LogLevel.Fatal, callerFilePath, callerMemberName, sourceLineNumber);
        }

        //LogLevel: 5
        public static void Error(object obj,
            [CallerFilePath]string callerFilePath = "",
            [CallerMemberName]string callerMemberName = "",
            [CallerLineNumber]int sourceLineNumber = 0)
        {
            Write(obj, LogLevel.Error, callerFilePath, callerMemberName, sourceLineNumber);
        }

        //LogLevel: 4
        public static void Warn(object obj,
            [CallerFilePath]string callerFilePath = "",
            [CallerMemberName]string callerMemberName = "",
            [CallerLineNumber]int sourceLineNumber = 0)
        {
            Write(obj, LogLevel.Warn, callerFilePath, callerMemberName, sourceLineNumber);
        }

        //LogLevel: 3
        public static void Info(object obj,
            [CallerFilePath]string callerFilePath = "",
            [CallerMemberName]string callerMemberName = "",
            [CallerLineNumber]int sourceLineNumber = 0)
        {
            Write(obj, LogLevel.Info, callerFilePath, callerMemberName, sourceLineNumber);
        }

        //LogLevel: 2
        public static void Debug(object obj,
            [CallerFilePath]string callerFilePath = "",
            [CallerMemberName]string callerMemberName = "",
            [CallerLineNumber]int sourceLineNumber = 0)
        {
            Write(obj, LogLevel.Debug, callerFilePath, callerMemberName, sourceLineNumber);
        }

        //LogLevel: 1
        public static void Trace(object obj,
            [CallerFilePath]string callerFilePath = "",
            [CallerMemberName]string callerMemberName = "",
            [CallerLineNumber]int sourceLineNumber = 0)
        {
            Write(obj, LogLevel.Trace, callerFilePath, callerMemberName, sourceLineNumber);
        }


        private static void Write(object obj, LogLevel logLevel, string callerFilePath, string callerMemberName, int sourceLineNumber)
        {
            var name = $"{Path.GetFileName(callerFilePath)} line {sourceLineNumber} / {callerMemberName}";
            var logger = LogManager.GetLogger(name);
            logger.Log(logLevel, obj);
        }
    }
}
