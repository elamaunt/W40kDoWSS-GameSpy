using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSMasterServer.DiscordBot.Database
{
    public static class DiscordDatabase
    {
        private static LiteDatabase _db;

        public static LiteCollection<DiscordProfile> ProfilesTable => _db.GetCollection<DiscordProfile>("DiscordProfiles");

        public static void InitDb()
        {
            _db = new LiteDatabase("Discord.db");
            /*var users = ProfilesTable.FindAll().ToList();
            foreach (var user in users)
            {
                user.RepChangingHistory = new Dictionary<ulong, long>();
            }
            ProfilesTable.Update(users);*/
        }

        public static DiscordProfile CreateDiscordProfile(ulong userId)
        {
            var profile = new DiscordProfile()
            {
                UserId = userId,
                SoftMuteUntil = 0,
                MuteUntil = 0,
                IsSoftMuteActive = false,
                IsMuteActive = false,
                Reputation = 0,
                RepChangingHistory = new Dictionary<ulong, long>()
            };
            ProfilesTable.Insert(profile);
            return profile;
        }

        public static DiscordProfile GetProfile(ulong userId)
        {
            var profile = ProfilesTable.FindOne(x => x.UserId == userId);
            return profile ?? CreateDiscordProfile(userId);
        }

        public static bool CanChangeReputation(ulong changerId, ulong targetId)
        {
            var profile = GetProfile(changerId);
            return !profile.RepChangingHistory.ContainsKey(targetId) ||
                   DateTime.UtcNow.Ticks >= profile.RepChangingHistory[targetId];
        }

        public static IEnumerable<DiscordProfile> GetTopByRating()
        {
            return ProfilesTable.FindAll().OrderByDescending(x => x.Reputation).Take(10);
        }

        public static void SetReputation(ulong userId, int reputation)
        {
            var profile = GetProfile(userId);
            profile.Reputation = reputation;
            ProfilesTable.Update(profile);
        }

        public static (int, int) ChangeRep(ulong targetId, ulong changerId, bool repAction)
        {
            if (!CanChangeReputation(changerId, targetId))
                return (int.MinValue, 0);
            var changerProfile = GetProfile(changerId);
            changerProfile.RepChangingHistory[targetId] = DateTime.UtcNow.AddDays(1).Ticks;
            ProfilesTable.Update(changerProfile);

            var profile = GetProfile(targetId);
            var repChange = BotExtensions.CalculateReputation(changerProfile.Reputation, repAction);
            profile.Reputation += repChange;
            ProfilesTable.Update(profile);
            return (repChange, profile.Reputation);
        }

        public static void RemoveMute(ulong userId, bool isSoftMute)
        {
            var profile = GetProfile(userId);
            if (isSoftMute)
                profile.IsSoftMuteActive = false;
            else
                profile.IsMuteActive = false;
            ProfilesTable.Update(profile);
        }

        public static void AddMute(ulong userId, long softMuteUntil, long muteUntil)
        {
            var profile = GetProfile(userId);

            if (softMuteUntil != 0)
            { 
                profile.SoftMuteUntil = softMuteUntil;
                profile.IsSoftMuteActive = true;
            }
            if (muteUntil != 0)
            {
                profile.MuteUntil = muteUntil;
                profile.IsMuteActive = true;
            }

            ProfilesTable.Update(profile);
        }
    }
}
