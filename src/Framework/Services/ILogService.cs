using System.Diagnostics;

namespace Framework
{
    public interface ILogService
    {
        void Write(object obj, StackFrame frame);
    }
}
