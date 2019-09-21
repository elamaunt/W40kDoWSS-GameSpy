using Discord;
using Discord.WebSocket;
using IrcNet.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSMasterServer.DiscordBot.Commands
{
    public class UnMuteCommand : IBotCommand
    {
        public AccessLevel MinAccessLevel { get; } = AccessLevel.Moderator;

        public async Task Execute(string[] commandParams, SocketMessage socketMessage)
        {
            var targetUsers = socketMessage.MentionedUsers;
            if (targetUsers.Count == 0)
                throw new Exception("[UnMuteCommand]No users were mentioned!");

            var server = (socketMessage.Channel as SocketGuildChannel).Guild;
            foreach (var user in targetUsers)
            {
                var guidUser = user as SocketGuildUser;
                var roles = new List<IRole>() { server.GetRole(DiscordServerConstants.readOnlyRoleId),  server.GetRole(DiscordServerConstants.floodOnlyRoleId)} ;
                await guidUser.RemoveRolesAsync(roles);
            }
        }
    }
}
