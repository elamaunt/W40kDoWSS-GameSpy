using Discord;
using Discord.WebSocket;
using IrcNet.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSMasterServer.DiscordBot.Database;

namespace GSMasterServer.DiscordBot.Commands
{
    internal class UnMuteCommand : IBotCommand
    {
        public AccessLevel MinAccessLevel { get; } = AccessLevel.Moderator;

        public AllowLevel AllowLevel { get; } = AllowLevel.Everywhere;

        private readonly bool _softUnmute;
        public UnMuteCommand(bool softUnmute)
        {
            _softUnmute = softUnmute;
        }

        public static async Task UnMute(IReadOnlyCollection<SocketUser> users, bool softUnmute, SocketGuild socketGuild)
        {
            foreach (var user in users)
            {
                var guidUser = user as SocketGuildUser;
                if (guidUser.GetAccessLevel() > AccessLevel.User) continue;
                var roleToRemove = softUnmute ? socketGuild.GetRole(DiscordServerConstants.FloodOnlyRoleId) : socketGuild.GetRole(DiscordServerConstants.ReadOnlyRoleId);
                await guidUser.RemoveRoleAsync(roleToRemove);
                DiscordDatabase.RemoveMute(user.Id, softUnmute);
                var logMessage = new StringBuilder();
                logMessage.Append($"<@{user.Id}> ");
                logMessage.Append(softUnmute
                    ? $"Congrats! You are not soft-muted anymore!"
                    : $"Congrats! You have been unmuted!");
                var channelToWrite = await user.GetOrCreateDMChannelAsync();
                await channelToWrite.SendMessageAsync(logMessage.ToString());
            }
        }

        public async Task Execute(SocketMessage socketMessage)
        {
            var targetUsers = socketMessage.MentionedUsers;
            if (targetUsers.Count == 0)
            {
                Logger.Trace("[UnMuteCommand]No users were mentioned!");
                return;
            }
            await UnMute(targetUsers, _softUnmute, ((SocketGuildChannel) socketMessage.Channel).Guild);

            await socketMessage.DeleteAsync();
        }
    }
}
