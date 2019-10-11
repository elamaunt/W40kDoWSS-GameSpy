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

        public static DiscordProfile GetProfile(ulong userId)
        {
            return ProfilesTable.FindOne(x => x.UserId == userId);
        }

        public static void RemoveMute(ulong userId, bool isSoftMute)
        {
            var profile = GetProfile(userId);
            if (profile == null) return;
            if (isSoftMute)
                profile.IsSoftMuteActive = false;
            else
                profile.IsMuteActive = false;
            ProfilesTable.Update(profile);
        }

        public static void AddMute(ulong userId, long softMuteUntil, long muteUntil)
        {
            var profile = GetProfile(userId);
            var isNew = profile == null;
            if (profile == null)
            {
                profile = new DiscordProfile()
                {
                    UserId = userId,
                    SoftMuteUntil = 0,
                    MuteUntil = 0,
                    IsSoftMuteActive = false,
                    IsMuteActive = false
                };
            }
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

            if (isNew)
                ProfilesTable.Insert(profile);
            else
                ProfilesTable.Update(profile);
        }
    }
}
