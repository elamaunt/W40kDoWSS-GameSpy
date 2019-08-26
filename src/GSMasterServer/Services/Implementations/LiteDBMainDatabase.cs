using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using GSMasterServer.Data;
using LiteDB;

namespace GSMasterServer.Services.Implementations
{
    public class LiteDBMainDatabase : IMainDataBase
    {
        LiteDatabase _db;

        LiteCollection<ProfileDBO> ProfilesTable => _db.GetCollection<ProfileDBO>("USERS");
        LiteCollection<NewsDBO> NewsTable => _db.GetCollection<NewsDBO>("NEWS");

        public void Initialize(string databasePath)
        {
            _db = new LiteDatabase(databasePath);
            
            _db.Engine.EnsureIndex("USERS", nameof(ProfileDBO.Name), true);
            _db.Engine.EnsureIndex("USERS", nameof(ProfileDBO.Score1v1));
            _db.Engine.EnsureIndex("USERS", nameof(ProfileDBO.Score2v2));
            _db.Engine.EnsureIndex("USERS", nameof(ProfileDBO.Score3v3));

            _db.Engine.EnsureIndex("NEWS", nameof(NewsDBO.Author), true);
            _db.Engine.EnsureIndex("NEWS", nameof(NewsDBO.CreatedDate), true);
            _db.Engine.EnsureIndex("NEWS", nameof(NewsDBO.EditedDate), true);
            _db.Engine.EnsureIndex("NEWS", nameof(NewsDBO.NewsType), true);
        }

        public bool IsInitialized => _db != null;
        
        public void Dispose()
        {
            _db?.Dispose();
            _db = null;
        }

        public ProfileDBO CreateProfile(string username, string passwordEncrypted, ulong steamId, string email, string country, IPAddress address)
        {
            var data = new ProfileDBO()
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

        public List<ProfileDBO> GetAllProfilesByEmailAndPass(string email, string passwordEncrypted)
        {
            var emailQ = Query.EQ(nameof(ProfileDBO.Email), new BsonValue(email));
            var passQuery = Query.EQ(nameof(ProfileDBO.Passwordenc), new BsonValue(email));

            var andQury = Query.And(emailQ, passQuery);

            return ProfilesTable.Find(andQury).ToList();
        }
        
        public ProfileDBO GetProfileByName(string username)
        {
            return ProfilesTable.FindOne(Query.EQ(nameof(ProfileDBO.Name), new BsonValue(username)));
        }

        public ProfileDBO GetProfileById(long profileId)
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

        public void UpdateProfileData(ProfileDBO stats)
        {
            ProfilesTable.Update(stats);
        }

        public bool ProfileExists(string username)
        {
           return ProfilesTable.Exists(Query.EQ(nameof(ProfileDBO.Name), new BsonValue(username)));
        }

        public ProfileDBO[] Load1v1Top10()
        {
            return ProfilesTable.Find(Query.All(nameof(ProfileDBO.Score1v1), Query.Descending), 0, 10).ToArray();
        }

        public ProfileDBO[] LoadAllStats()
        {
            return ProfilesTable.Find(Query.All(nameof(ProfileDBO.Score1v1), Query.Descending), 0).ToArray();
        }

        public NewsDBO[] GetLastNews(int count)
        {
            return NewsTable.Find(Query.All(nameof(NewsDBO.CreatedDate), Query.Descending), 0, count).ToArray();
        }
    }
}
