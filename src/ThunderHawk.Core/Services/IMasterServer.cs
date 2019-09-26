using System;

namespace ThunderHawk.Core
{
    public interface IMasterServer
    {
        void Connect(ulong steamId);
        bool StateSynced { get; }
        UserInfo[] GetAllUsers();

        StatsInfo GetStatsInfo(long profileId);

        void RequestGameBroadcast();
        void RequestUserStats(long profileId);
        void RequestUserStats(string name);
        void SendChatMessage(string text);
        void RequestUsers();

        event Action ConnectionLost;
        event Action ConnectionRestored;
        event Action UsersLoaded;
        event Action<UserInfo> UserDisconnected;
        event Action<UserInfo> UserConnected;
        event Action<UserInfo> UserChanged;

        event Action<MessageInfo> ChatMessageReceived;
        void Disconnect();
    }
}
