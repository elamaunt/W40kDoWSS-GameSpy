using System;

namespace ThunderHawk.Core
{
    public interface IMasterServer
    {
        void Connect(ulong steamId);
        bool IsConnected { get; }
        UserInfo[] GetAllUsers();

        StatsInfo GetStatsInfo(long profileId);

        void RequestGameBroadcast(bool teamplay, string gameVariant, int maxPlayers, int players, bool ranked);
        void RequestUserStats(long profileId);
        void RequestUserStats(string name);
        void SendChatMessage(string text);
        void RequestUsers();

        string ModName { get; }
        string ModVersion { get; }
        string ActiveGameVariant { get; }

        event Action ConnectionLost;
        event Action Connected;
        event Action UsersLoaded;
        event Action<UserInfo> UserDisconnected;
        event Action<UserInfo> UserConnected;
        event Action<UserInfo> UserChanged;

        event Action<MessageInfo> ChatMessageReceived;
        void Disconnect();
    }
}
