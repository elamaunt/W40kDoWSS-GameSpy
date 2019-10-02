using Discord;
using Discord.WebSocket;
using IrcNet.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSMasterServer.DiscordBot.Commands
{
    public class UnMuteCommand : IBotCommand
    {
        public AccessLevel MinAccessLevel { get; } = AccessLevel.Moderator;

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
                var roleToRemove = softUnmute ? socketGuild.GetRole(DiscordServerConstants.floodOnlyRoleId) : socketGuild.GetRole(DiscordServerConstants.readOnlyRoleId);
                await guidUser.RemoveRoleAsync(roleToRemove);
            }
            var logMessage = new StringBuilder();
            foreach (var user in users)
            {
                logMessage.Append($"<@{user.Id}> ");
            }
            logMessage.Append($"Congrats! You have been unmuted!");
            await BotMain.WriteToLogChannel(logMessage.ToString());
        }

        public async Task Execute(SocketMessage socketMessage)
        {
            var targetUsers = socketMessage.MentionedUsers;
            if (targetUsers.Count == 0)
                throw new Exception("[UnMuteCommand]No users were mentioned!");
            await UnMute(targetUsers, _softUnmute, (socketMessage.Channel as SocketGuildChannel).Guild);

            await socketMessage.DeleteAsync();
        }
    }
}
