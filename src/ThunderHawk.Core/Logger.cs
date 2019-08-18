using Framework;
using NLog;
using System.Runtime.CompilerServices;

namespace ThunderHawk.Core
{
    public static class Logger
    {
        static ILogService LogService { get; } = Service<ILogService>.Get();
       
        //LogLevel: 6
        public static void Fatal(object obj,
            [CallerFilePath]string callerFilePath = "",
            [CallerMemberName]string callerMemberName = "",
            [CallerLineNumber]int sourceLineNumber = 0)
        {
            LogService.Write(obj, LogLevel.Fatal, callerFilePath, callerMemberName, sourceLineNumber);
        }

        //LogLevel: 5
        public static void Error(object obj,
            [CallerFilePath]string callerFilePath = "",
            [CallerMemberName]string callerMemberName = "",
            [CallerLineNumber]int sourceLineNumber = 0)
        {
            LogService.Write(obj, LogLevel.Error, callerFilePath, callerMemberName, sourceLineNumber);
        }

        //LogLevel: 4
        public static void Warn(object obj,
            [CallerFilePath]string callerFilePath = "",
            [CallerMemberName]string callerMemberName = "",
            [CallerLineNumber]int sourceLineNumber = 0)
        {
            LogService.Write(obj, LogLevel.Warn, callerFilePath, callerMemberName, sourceLineNumber);
        }

        //LogLevel: 3
        public static void Info(object obj,
            [CallerFilePath]string callerFilePath = "",
            [CallerMemberName]string callerMemberName = "",
            [CallerLineNumber]int sourceLineNumber = 0)
        {
            LogService.Write(obj, LogLevel.Info, callerFilePath, callerMemberName, sourceLineNumber);
        }

        //LogLevel: 2
        public static void Debug(object obj,
            [CallerFilePath]string callerFilePath = "",
            [CallerMemberName]string callerMemberName = "",
            [CallerLineNumber]int sourceLineNumber = 0)
        {
            LogService.Write(obj, LogLevel.Debug, callerFilePath, callerMemberName, sourceLineNumber);
        }

        //LogLevel: 1
        public static void Trace(object obj,
            [CallerFilePath]string callerFilePath = "",
            [CallerMemberName]string callerMemberName = "",
            [CallerLineNumber]int sourceLineNumber = 0)
        {
            LogService.Write(obj, LogLevel.Trace, callerFilePath, callerMemberName, sourceLineNumber);
        }
    }
}
