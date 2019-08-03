using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using GSMasterServer.Data;
using LiteDB;

namespace GSMasterServer.Services.Implementations
{
    public class LiteDBUsersDatabase : IUsersDataBase
    {
        private const string Category = "UsersDatabase";

        LiteDatabase _db;

        LiteCollection<UserData> UsersTable => _db.GetCollection<UserData>("USERS");
        LiteCollection<StatsData> StatsTable => _db.GetCollection<StatsData>("STATS");

        private const int UserIdOffset = 200000000;
        private const int ProfileIdOffset = 100000000;

        public void Initialize(string databasePath)
        {
            _db = new LiteDatabase(databasePath);

            _db.Engine.EnsureIndex("USERS", nameof(UserData.ProfileId), true);
            _db.Engine.EnsureIndex("USERS", nameof(UserData.UserId), true);
            _db.Engine.EnsureIndex("USERS", nameof(UserData.Name), true);

            _db.Engine.EnsureIndex("STATS", nameof(StatsData.ProfileId), true);
            _db.Engine.EnsureIndex("STATS", nameof(StatsData.Score1v1));
            _db.Engine.EnsureIndex("STATS", nameof(StatsData.Score2v2));
            _db.Engine.EnsureIndex("STATS", nameof(StatsData.Score3v3));
        }

        public bool IsInitialized => _db != null;



        public void Dispose()
        {
            _db?.Dispose();
            _db = null;
        }

        public UserData CreateUser(string username, string passwordEncrypted, ulong steamId, string email, string country, IPAddress address)
        {
            var data = new UserData()
            {
                Name = username,
                LastIp = address.ToString(),
                Country = country,
                Passwordenc = passwordEncrypted,
                Email = email,
                SteamId = steamId
            };

            var idBson = UsersTable.Insert(data);

            data.Id = idBson.AsInt64;
            data.ProfileId = idBson.AsInt64 + ProfileIdOffset;
            data.UserId = idBson.AsInt64 + UserIdOffset;
            
            UsersTable.Update(data);

            var stats = new StatsData()
            {
                Id = data.Id,
                ProfileId = idBson.AsInt64 + ProfileIdOffset,
                UserId = idBson.AsInt64 + UserIdOffset,
                Score1v1 = 1000,
                Score2v2 = 1000,
                Score3v3 = 1000,
                Modified = DateTime.UtcNow.Ticks
            };

            StatsTable.Insert(new BsonValue(data.Id), stats);

            return data;
        }

        public List<UserData> GetAllUserDatas(string email, string passwordEncrypted)
        {
            var emailQ = Query.EQ(nameof(UserData.Email), new BsonValue(email));
            var passQuery = Query.EQ(nameof(UserData.Passwordenc), new BsonValue(email));

            var andQury = Query.And(emailQ, passQuery);

            return UsersTable.Find(andQury).ToList();
        }

        public StatsData GetStatsDataByNick(string nick)
        {
            return StatsTable.FindOne(Query.EQ(nameof(UserData.Name), new BsonValue(nick)));
        }

        public StatsData GetStatsDataByProfileId(long profileId)
        {
            return StatsTable.FindById(new BsonValue(profileId - ProfileIdOffset));
        }

        public UserData GetUserData(string username)
        {
            return UsersTable.FindOne(Query.EQ(nameof(UserData.Name), new BsonValue(username)));
        }

        public UserData GetUserDataByProfileId(long profileId)
        {
            return UsersTable.FindById(new BsonValue(profileId - ProfileIdOffset));
        }
        
        public void LogLogin(string name, ulong steamId, IPAddress address)
        {
            var data = GetUserData(name);

            if (data == null)
                return;

            data.SteamId = steamId;
            data.LastIp = address.ToString();

            UsersTable.Update(data);
        }

        /*public void SetUserData(string name, Dictionary<string, object> data)
        {
            throw new System.NotImplementedException();
        }*/

        public void UpdateUserStats(StatsData stats)
        {
            StatsTable.Update(stats);
        }

        public bool UserExists(string username)
        {
           return UsersTable.Exists(Query.EQ(nameof(UserData.Name), new BsonValue(username)));
        }

        public KeyValuePair<string, StatsData>[] Load1v1Top10()
        {
            return StatsTable.Find(Query.All(nameof(StatsData.Score1v1), Query.Descending), 0, 10).Select(x => new KeyValuePair<string, StatsData>(UsersTable.FindById(new BsonValue(x.Id)).Name, x)).ToArray();
        }

        public KeyValuePair<string, StatsData>[] LoadAllStats()
        {
            return StatsTable.Find(Query.All(nameof(StatsData.Score1v1), Query.Descending), 0).Select(x => new KeyValuePair<string, StatsData>(UsersTable.FindById(new BsonValue(x.Id)).Name, x)).ToArray();
        }

    }
}
