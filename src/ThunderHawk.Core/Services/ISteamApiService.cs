using System;

namespace ThunderHawk.Core
{
    public interface ISteamApiService
    {
        void Initialize();

        bool IsInitialized { get; }

        string NickName { get; }

        string GetUserName(ulong steamId);

        event Action<ulong> UserStateChanged;
        event Action<ulong> UserRichPresenceChanged;
    }
}
