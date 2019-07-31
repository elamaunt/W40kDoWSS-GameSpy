using GSMasterServer.Data;
using System;
using System.Collections.Generic;
using System.Net;

namespace GSMasterServer.Services
{
    public interface IUsersDataBase : IDisposable
    {
        bool IsInitialized { get; }
        void Initialize(string databasePath);

        UserData GetUserDataByProfileId(long profileId);
        StatsData GetStatsDataByNick(string nick);

        void UpdateUserStats(StatsData stats);

        StatsData GetStatsDataByProfileId(long profileId);

        UserData GetUserData(string username);

        List<UserData> GetAllUserDatas(string email, string passwordEncrypted);

        //void SetUserData(string name, Dictionary<string, object> data);

        void LogLogin(string name, ulong steamId, IPAddress address);

        UserData CreateUser(string username, string passwordEncrypted, ulong steamId, string email, string country, IPAddress address);

        bool UserExists(string username);
    }
}