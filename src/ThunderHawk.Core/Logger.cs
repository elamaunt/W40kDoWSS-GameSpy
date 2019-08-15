using Framework;
using System.Runtime.CompilerServices;
using NLog;
using System.Diagnostics;

namespace ThunderHawk.Core
{
    public static class Logger
    {
       static ILogService LogService { get; } = Service<ILogService>.Get();
       
        public static void Log(object obj, LogLevel logLevel = null,
            [CallerFilePath]string callerFilePath = "",
            [CallerMemberName]string callerMemberName = "", 
            [CallerLineNumber]int sourceLineNumber = 0)
        {
            LogService.Write(obj, logLevel, callerFilePath, callerMemberName, sourceLineNumber);
        }
    }
}
