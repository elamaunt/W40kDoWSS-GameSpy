using System;

namespace ThunderHawk.Core
{
    public interface ISteamApiService
    {
        void Initialize();

        bool IsInitialized { get; }

        string NickName { get; }
        ulong SteamId { get; }

        string GetUserName(ulong steamId);

        event Action<ulong> UserStateChanged;
        event Action<ulong> UserRichPresenceChanged;

        ulong GetUserSteamId();
        DateTime GetCurrentTime();
        void TestConnectionWithPlayer(ulong steamId);
        void ResetPortBindingWithPlayer(ulong steamId);
    }
}
