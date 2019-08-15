using Framework;
using NLog;
using System.Diagnostics;

namespace ThunderHawk.Core
{
    public static class Logger
    {
       static ILogService LogService { get; } = Service<ILogService>.Get();

        public static void Log(object obj, LogLevel logLevel = null)
        {
            LogService.Write(obj, new StackFrame(1, false), logLevel);
        }
    }
}
