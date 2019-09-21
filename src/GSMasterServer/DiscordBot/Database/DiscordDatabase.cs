using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSMasterServer.DiscordBot.Database
{
    public static class DiscordDatabase
    {
        private static LiteDatabase db;

        private static LiteCollection<DiscordProfile> ProfilesTable => db.GetCollection<DiscordProfile>("DiscordProfiles");

        public static void InitDb()
        {
            db = new LiteDatabase("Discord.db");
        }

        public static DiscordProfile GetProfile(ulong userId)
        {
            return ProfilesTable.FindOne(x => x.UserId == userId);
        }

        public static void UpdateData(ulong userId, ulong softMuteUntil, ulong muteUntil)
        {
            var data = new DiscordProfile()
            {
                UserId = userId,
                MuteUntil = muteUntil,
                SoftMuteUntil = softMuteUntil
            };

            if (GetProfile(userId) != null)
                ProfilesTable.Update(data);
            else
            {
                ProfilesTable.Insert(data);
            }
        }
    }
}
