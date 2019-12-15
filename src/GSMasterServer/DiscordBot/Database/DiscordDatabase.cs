﻿using LiteDB;
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
            };
            ProfilesTable.Insert(profile);
            return profile;
        }

        public static DiscordProfile GetProfile(ulong userId)
        {
            var profile = ProfilesTable.FindOne(x => x.UserId == userId);
            return profile ?? CreateDiscordProfile(userId);
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
