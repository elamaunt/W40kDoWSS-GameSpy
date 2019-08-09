using Framework;
using System.Diagnostics;

namespace ThunderHawk.Core
{
    public static class Logger
    {
       static ILogService LogService { get; } = Service<ILogService>.Get();

        public static void Log(object obj)
        {
            LogService.Write(obj, new StackFrame(1, false));
        }
    }
}
