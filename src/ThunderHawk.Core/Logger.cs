using Framework;
using System.Runtime.CompilerServices;

namespace ThunderHawk.Core
{
    public static class Logger
    {
       static ILogService LogService { get; } = Service<ILogService>.Get();
        public static void Log(object obj,
            [CallerFilePath]string callerFilePath = "",
            [CallerMemberName]string callerMemberName = "", 
            [CallerLineNumber]int sourceLineNumber = 0)
        {
            LogService.Write(obj, callerFilePath, callerMemberName, sourceLineNumber);
        }
    }
}
