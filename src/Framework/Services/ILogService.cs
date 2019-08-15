using NLog;
using System.Diagnostics;

namespace Framework
{
    public interface ILogService
    {
        void Write(object obj, LogLevel logLevel, string callerFilePath, string callerMemberName, int sourceLineNumber);
    }
}
