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

        LiteCollection<GameDBO> GamesTable => _db.GetCollection<GameDBO>("GAMES");
        LiteCollection<ProfileDBO> ProfilesTable => _db.GetCollection<ProfileDBO>("USERS");
        LiteCollection<NewsDBO> NewsTable => _db.GetCollection<NewsDBO>("NEWS");
        LiteCollection<Profile1X1DBO> Profiles1X1Table => _db.GetCollection<Profile1X1DBO>("PROFILES1X1");
        LiteCollection<Profile2X2DBO> Profiles2X2Table => _db.GetCollection<Profile2X2DBO>("PROFILES2X2");
        LiteCollection<Profile3X3DBO> Profiles3X3Table => _db.GetCollection<Profile3X3DBO>("PROFILES3X3");
        LiteCollection<Profile4X4DBO> Profiles4X4Table => _db.GetCollection<Profile4X4DBO>("PROFILES4X4");

        public void Initialize(string databasePath)
        {
            _db = new LiteDatabase(databasePath);

            _db.Engine.EnsureIndex("USERS", nameof(ProfileDBO.Name), true);
            _db.Engine.EnsureIndex("USERS", nameof(ProfileDBO.Score1v1));
            _db.Engine.EnsureIndex("USERS", nameof(ProfileDBO.Score2v2));
            _db.Engine.EnsureIndex("USERS", nameof(ProfileDBO.Score3v3));
            _db.Engine.EnsureIndex("USERS", nameof(ProfileDBO.SteamId));
            _db.Engine.EnsureIndex("USERS", nameof(ProfileDBO.Modified));

            _db.Engine.EnsureIndex("GAMES", nameof(GameDBO.UploadedDate), false);
            _db.Engine.EnsureIndex("GAMES", nameof(GameDBO.ModName), false);
            _db.Engine.EnsureIndex("GAMES", nameof(GameDBO.ModVersion), false);
            _db.Engine.EnsureIndex("GAMES", nameof(GameDBO.Type), false);
            _db.Engine.EnsureIndex("GAMES", nameof(GameDBO.UploadedBy), false);
            _db.Engine.EnsureIndex("GAMES", nameof(GameDBO.Duration), false);

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

        public ProfileDBO CreateProfile(string username, string passwordEncrypted, ulong steamId, string email,
            string country, IPAddress address)
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

        public List<ProfileDBO> GetAllProfilesBySteamId(ulong steamId)
        {
            return ProfilesTable.Find(Query.EQ(nameof(ProfileDBO.SteamId), new BsonValue((long)steamId))).ToList();

            /*var emailQ = Query.EQ(nameof(ProfileDBO.Email), new BsonValue(email));
            var passQuery = Query.EQ(nameof(ProfileDBO.Passwordenc), new BsonValue(email));

            var andQury = Query.And(emailQ, passQuery);

            return ProfilesTable.Find(andQury).ToList();*/
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
            data.Modified = DateTime.UtcNow.Ticks;

            ProfilesTable.Update(data);
        }

        public void UpdateProfileData(ProfileDBO stats)
        {
            stats.Modified = DateTime.UtcNow.Ticks;
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

        /*
            detail stats tables api                                               
         */
        // 1x1 api
        public Profile1X1DBO GetProfile1X1ByProfileId(long profileId)
        {
            return Profiles1X1Table.FindOne(Query.EQ(nameof(Profile1X1DBO.ProfileId), new BsonValue(profileId)));
        }

        public Profile1X1DBO CreateProfile1X1(long profileId)
        {
            var data = new Profile1X1DBO(profileId);
            Profiles1X1Table.Insert(data);
            return data;
        }

        public void UpdateProfile1X1(Profile1X1DBO profile1X1)
        {
            Profiles1X1Table.Update(profile1X1);
        }

        // 2x2 api
        public Profile2X2DBO GetProfile2X2ByProfileId(long profileId)
        {
            return Profiles2X2Table.FindOne(Query.EQ(nameof(Profile2X2DBO.ProfileId), new BsonValue(profileId)));
        }

        public Profile2X2DBO CreateProfile2X2(long profileId)
        {
            var data = new Profile2X2DBO(profileId);
            Profiles2X2Table.Insert(data);
            return data;
        }

        public void UpdateProfile2X2(Profile2X2DBO profile2X2)
        {
            Profiles2X2Table.Update(profile2X2);
        }

        // 3x3 api
        public Profile3X3DBO GetProfile3X3ByProfileId(long profileId)
        {
            return Profiles3X3Table.FindOne(Query.EQ(nameof(Profile3X3DBO.ProfileId), new BsonValue(profileId)));
        }

        public Profile3X3DBO CreateProfile3X3(long profileId)
        {
            var data = new Profile3X3DBO(profileId);
            Profiles3X3Table.Insert(data);
            return data;
        }

        public void UpdateProfile3X3(Profile3X3DBO profile3X3)
        {
            Profiles3X3Table.Update(profile3X3);
        }

        // 4x4 api
        public Profile4X4DBO GetProfile4X4ByProfileId(long profileId)
        {
            return Profiles4X4Table.FindOne(Query.EQ(nameof(Profile4X4DBO.ProfileId), new BsonValue(profileId)));
        }

        public Profile4X4DBO CreateProfile4X4(long profileId)
        {
            var data = new Profile4X4DBO(profileId);
            Profiles4X4Table.Insert(data);
            return data;
        }

        public void UpdateProfile4X4(Profile4X4DBO profile4X4)
        {
            Profiles4X4Table.Update(profile4X4);
        }

        public NewsDBO[] GetLastNews(int count)
        {
            return NewsTable.Find(Query.All(nameof(NewsDBO.CreatedDate), Query.Descending), 0, count).ToArray();
        }

        public GameDBO[] GetLastGames()
        {
            return GamesTable.Find(Query.All(nameof(GameDBO.UploadedDate), Query.Descending), 0, 10).ToArray();

        }


        public IEnumerable<ProfileDBO> GetProfilesBySteamId(long steamId)
        {
            return ProfilesTable.Find(Query.And(Query.EQ(nameof(ProfileDBO.SteamId), new BsonValue(steamId)), Query.All(nameof(ProfileDBO.Modified), Query.Descending)), 0);
        }

        public bool TryRegisterGame(ref GameDBO game)
        {
            var gameInDb = GamesTable.FindById(new BsonValue(game.Id));

            if (gameInDb != null)
            {
                game = gameInDb;
                return false;
            }

            return GamesTable.Upsert(game);
        }
    }
}