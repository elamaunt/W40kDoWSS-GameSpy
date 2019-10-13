﻿using Discord;
using Discord.WebSocket;
using GSMasterServer.DiscordBot.Database;
using IrcNet.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSMasterServer.DiscordBot.Commands
{
    public class MuteCommand : IBotCommand
    {
        public AccessLevel MinAccessLevel { get; } = AccessLevel.Moderator;

        private readonly bool _softMute;
        public MuteCommand(bool softMute)
        {
            _softMute = softMute;
        }

        public static async Task<List<SocketUser>> Mute(IReadOnlyCollection<SocketUser> users, bool softMute, SocketGuild socketGuild, long muteUntil)
        {
            var roleId = softMute ? DiscordServerConstants.floodOnlyRoleId : DiscordServerConstants.readOnlyRoleId;
            var mutedUsers = new List<SocketUser>();
            foreach (var user in users)
            {
                if (!(user is SocketGuildUser guidUser)) continue;;
                if (guidUser.GetAccessLevel() > AccessLevel.User) continue;
                await guidUser.AddRoleAsync(socketGuild.GetRole(roleId));
                DiscordDatabase.AddMute(guidUser.Id, softMute ? muteUntil : 0, !softMute ? muteUntil : 0);
                mutedUsers.Add(guidUser);
            }

            return mutedUsers;
        }


        /// <summary>
        /// Possible command params:
        /// (optionally) How long (ulong minutes)
        /// If not passed, user will be muted forever
        /// </summary>
        /// <param name="socketMessage"></param>
        /// <returns></returns>
        public async Task Execute(SocketMessage socketMessage)
        {
            string[] commandParams = socketMessage.CommandArgs();
            var paramCount = commandParams.Length;
            ulong howLong = 0;
            if (paramCount > 0)
            {
                ulong.TryParse(commandParams[0], out howLong);
            }
            var targetUsers = socketMessage.MentionedUsers;
            if (targetUsers.Count == 0)
            {
                Logger.Debug("[MuteCommand]No user were mentioned");
                return;
            }

            var guild = ((SocketGuildChannel) socketMessage.Channel).Guild;
            List<SocketUser> mutedUsers;
            if (howLong != 0)
            {
                var timeUntilMute = DateTime.UtcNow.AddMinutes(howLong).Ticks;
                mutedUsers = await Mute(targetUsers, _softMute, guild, timeUntilMute);
            }
            else
            {
                mutedUsers = await Mute(targetUsers, _softMute, guild, -1);
            }

            foreach (var user in mutedUsers)
            {
                var logMessage = new StringBuilder();
                logMessage.Append($"<@{user.Id}> ");
                logMessage.Append(howLong != 0
                    ? $"You've been {(_softMute ? "soft-" : "")}muted for {howLong} minutes"
                    : $"You've been {(_softMute ? "soft-" : "")}muted FOREVER!");
                await BotMain.WriteToLogChannel(logMessage.ToString());

            }
            await socketMessage.DeleteAsync();
        }
    }
}
