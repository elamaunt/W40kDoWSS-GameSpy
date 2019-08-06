using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using GSMasterServer.Data;
using LiteDB;

namespace GSMasterServer.Services.Implementations
{
    public class LiteDBProfilesDatabase : IProfilesDataBase
    {
        private const string Category = "UsersDatabase";

        LiteDatabase _db;

        LiteCollection<ProfileData> ProfilesTable => _db.GetCollection<ProfileData>("USERS");

        public void Initialize(string databasePath)
        {
            _db = new LiteDatabase(databasePath);
            
            _db.Engine.EnsureIndex("USERS", nameof(ProfileData.Name), true);
            _db.Engine.EnsureIndex("USERS", nameof(ProfileData.Score1v1));
            _db.Engine.EnsureIndex("USERS", nameof(ProfileData.Score2v2));
            _db.Engine.EnsureIndex("USERS", nameof(ProfileData.Score3v3));
        }

        public bool IsInitialized => _db != null;
        
        public void Dispose()
        {
            _db?.Dispose();
            _db = null;
        }

        public ProfileData CreateProfile(string username, string passwordEncrypted, ulong steamId, string email, string country, IPAddress address)
        {
            var data = new ProfileData()
            {
                Name = username,
                LastIp = address.ToString(),
                Country = country,
                Passwordenc = passwordEncrypted,
                Email = email,
                SteamId = steamId,
                Score1v1 = 1000,
                Score2v2 = 1000,
                Score3v3 = 1000,
                Modified = DateTime.UtcNow.Ticks
            };

            var idBson = ProfilesTable.Insert(data);

            data.Id = idBson.AsInt64;
           

            ProfilesTable.Update(data);
            
            return data;
        }
        
        public bool AddFriend(long toProfileId, long friendProfileId)
        {
            var profile = ProfilesTable.FindById(new BsonValue(toProfileId));

            if (profile == null)
                return false;

            if (profile.Friends == null)
                profile.Friends = new List<long>();

            if (!profile.Friends.Contains(friendProfileId))
            {
                profile.Friends.Add(friendProfileId);
                ProfilesTable.Update(profile);
                return true;
            }

            return false;
        }

        public void RemoveFriend(long profileId, long friendProfileId)
        {
            var profile = ProfilesTable.FindById(new BsonValue(profileId));

            if (profile == null)
                return;

            if (profile.Friends == null)
                return;

            if (profile.Friends.Remove(friendProfileId))
                ProfilesTable.Update(profile);
        }

        public List<ProfileData> GetAllProfilesByEmailAndPass(string email, string passwordEncrypted)
        {
            var emailQ = Query.EQ(nameof(ProfileData.Email), new BsonValue(email));
            var passQuery = Query.EQ(nameof(ProfileData.Passwordenc), new BsonValue(email));

            var andQury = Query.And(emailQ, passQuery);

            return ProfilesTable.Find(andQury).ToList();
        }
        
        public ProfileData GetProfileByName(string username)
        {
            return ProfilesTable.FindOne(Query.EQ(nameof(ProfileData.Name), new BsonValue(username)));
        }

        public ProfileData GetProfileById(long profileId)
        {
            return ProfilesTable.FindById(new BsonValue(profileId));
        }
        
        public void LogProfileLogin(string name, ulong steamId, IPAddress address)
        {
            var data = GetProfileByName(name);

            if (data == null)
                return;

            data.SteamId = steamId;
            data.LastIp = address.ToString();

            ProfilesTable.Update(data);
        }

        public void UpdateProfileData(ProfileData stats)
        {
            ProfilesTable.Update(stats);
        }

        public bool ProfileExists(string username)
        {
           return ProfilesTable.Exists(Query.EQ(nameof(ProfileData.Name), new BsonValue(username)));
        }

        public ProfileData[] Load1v1Top10()
        {
            return ProfilesTable.Find(Query.All(nameof(ProfileData.Score1v1), Query.Descending), 0, 10).ToArray();
        }

        public ProfileData[] LoadAllStats()
        {
            return ProfilesTable.Find(Query.All(nameof(ProfileData.Score1v1), Query.Descending), 0).ToArray();
        }

    }
}
