using System;

namespace ThunderHawk.Core
{
    public class OpenLogsService : IOpenLogsService
    {
        public event Action<string> LogMessageReceived;

        public void Log(string message)
        {
            LogMessageReceived?.Invoke(message);
        }
    }
}
