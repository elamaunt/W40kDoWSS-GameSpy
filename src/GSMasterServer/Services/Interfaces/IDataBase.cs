using System;

namespace GSMasterServer
{
    public interface IDataBase : IDisposable
    {
        void Initialize(string databasePath);
        bool IsInitialized { get; }
    }
}
