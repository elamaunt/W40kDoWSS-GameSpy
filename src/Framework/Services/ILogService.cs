using System.Diagnostics;

namespace Framework
{
    public interface ILogService
    {
        void Write(object obj, string callerFilePath, string callerMemberName, int sourceLineNumber);
    }
}
