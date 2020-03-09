using LiteDB;

namespace DiscordBot.Database
{
    internal static class BotDatabase
    {
        private static LiteDatabase _db;

        public static LiteCollection<UserProfile> ProfilesTable => _db.GetCollection<UserProfile>("UserProfiles");

        public static void InitDb()
        {
            _db = new LiteDatabase("DiscordData.db");
        }

        public static void DeInitDb()
        {
            _db.Dispose();
            _db = null;
        }

        public static UserProfile CreateDiscordProfile(ulong discordUserId)
        {
            var profile = new UserProfile
            {
                DiscordUserId = discordUserId,
                MuteUntil = 0,
                IsMuteActive = false,
                IsRussianLanguage = false,
            };
            ProfilesTable.Insert(profile);
            return profile;
        }

        public static bool IsUserRussian(ulong discordUserId)
        {
            var profile = GetProfile(discordUserId);
            return profile.IsRussianLanguage;
        }

        public static void SetUserLanguage(ulong discordUserId, bool isRusLanguage)
        {
            var profile = GetProfile(discordUserId);
            profile.IsRussianLanguage = isRusLanguage;
            ProfilesTable.Update(profile);
        }

        public static UserProfile GetProfile(ulong discordUserId)
        {
            var profile = ProfilesTable.FindOne(x => x.DiscordUserId == discordUserId);
            return profile ?? CreateDiscordProfile(discordUserId);
        }


        public static void RemoveMute(ulong userId)
        {
            var profile = GetProfile(userId);
            profile.IsMuteActive = false;
            ProfilesTable.Update(profile);
        }

        public static void AddMute(ulong userId, long muteUntil)
        {
            var profile = GetProfile(userId);
            
            if (muteUntil != 0)
            {
                profile.MuteUntil = muteUntil;
                profile.IsMuteActive = true;
            }

            ProfilesTable.Update(profile);
        }
    }
}