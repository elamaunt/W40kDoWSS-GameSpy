using System;
using SharedServices;

namespace ThunderHawk.Core
{
    public interface IMasterServer
    {
        UserInfo CurrentProfile { get; }

        void Connect(ulong steamId);
        bool IsConnected { get; }

        bool IsLastGamesLoaded { get; }
        bool IsPlayersTopLoaded { get; }
        UserInfo[] GetAllUsers();

        StatsInfo GetStatsInfo(long profileId);
        StatsInfo GetStatsInfo(string nick);
        void RequestPlayersTop(int offset, int count);
        void RequestGameBroadcast(bool teamplay, string gameVariant, int maxPlayers, int players, bool ranked);
        void RequestLastGames();
        StatsInfo[] GetPlayersTop(int offset, int count);
        void RequestUserStats(long profileId);
        void RequestUserStats(string name);
        void RequestAllUserNicks(string email);
        void RequestNameCheck(string name);

        void SendChatMessage(string text, bool fromGame);
        void RequestUsers();
        void SendGameFinishedInfo(GameFinishedMessage message);

        string ModName { get; }
        string ModVersion { get; }
        string ActiveGameVariant { get; }

        //ulong ActiveProfileId { get; }
        ///ulong ActiveProfileName { get; }

        event Action<string[]> NicksReceived;
        event Action<string, long?> NameCheckReceived;
        event Action<string> LoginErrorReceived;
        
        event Action ConnectionLost;
        event Action Connected;
        event Action UsersLoaded;
        event Action<string, string, string> UserKeyValueChanged;
        event Action<UserInfo> UserDisconnected;
        event Action<UserInfo> UserConnected;
        event Action<UserInfo, string, string> UserNameChanged;
        event Action<UserInfo> UserChanged;
        event Action<LoginInfo> LoginInfoReceived;
        event Action<StatsInfo[], int, int> PlayersTopLoaded;
        event Action<GameInfo[]> LastGamesLoaded;
        event Action<GameInfo> NewGameReceived;

        event Action<MessageInfo> ChatMessageReceived;
        void Disconnect();
        void RequestLogout();
        void RequestLogin(string name);
        LoginInfo GetLoginInfo(string name);
        UserInfo GetUserInfo(ulong steamId);

        GameInfo[] LastGames { get; }
        StatsInfo[] PlayersTop { get; }

        void SendKeyValuesChanged(string[] pairs);
    }
}
