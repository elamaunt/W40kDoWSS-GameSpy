using System;
using System.IO;
using System.Runtime.CompilerServices;
using NLog;

namespace GSMasterServer.Services
{
    class Logger
    {
        
        public static void Trace(string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            getLogger(callerFilePath, callerMemberName, sourceLineNumber)
                .Trace(message);
        }
        
        public static void Debug(string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            getLogger(callerFilePath, callerMemberName, sourceLineNumber)
                .Debug(message);
        }
        
        public static void Info(string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            getLogger(callerFilePath, callerMemberName, sourceLineNumber)
                .Info(message);
        }
        
        public static void Warn(string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            getLogger(callerFilePath, callerMemberName, sourceLineNumber)
                .Warn(message);
        }
        
        public static void Error(Exception ex,
            string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            getLogger(callerFilePath, callerMemberName, sourceLineNumber)
                .Error($" {message}: {ex.StackTrace}");
        }
        
        public static void Error(string message,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            getLogger(callerFilePath, callerMemberName, sourceLineNumber)
                .Error(message);
        }

        private static NLog.Logger getLogger(string callerFilePath, string callerMemberName, int sourceLineNumber)
        {
            var name = $"{Path.GetFileName(callerFilePath)} / {callerMemberName} / {sourceLineNumber}";
            return LogManager.GetLogger(name);
        }
    }
}
