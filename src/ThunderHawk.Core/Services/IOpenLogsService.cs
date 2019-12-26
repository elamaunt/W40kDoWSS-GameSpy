using System;

namespace ThunderHawk.Core
{
    public interface IOpenLogsService
    {
        void Log(string message);

        event Action<string> LogMessageReceived;
    }
}
