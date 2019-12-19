using System;

namespace ThunderHawk.Core
{
    public interface IClientServer
    {
        event Action<GameHostInfo[]> LobbiesUpdatedByRequest;

        string GetIndicator();
        void SendAsServerMessage(string message);

        void Start();
        void Stop();
    }
}
