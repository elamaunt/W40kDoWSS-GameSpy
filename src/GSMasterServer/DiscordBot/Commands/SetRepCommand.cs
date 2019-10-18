using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using GSMasterServer.DiscordBot.Database;
using IrcNet.Tools;

namespace GSMasterServer.DiscordBot.Commands
{
    public class SetRepCommand : IBotCommand
    {
        public AccessLevel MinAccessLevel { get; } = AccessLevel.Admin;

        public async Task Execute(SocketMessage socketMessage)
        {
            var commandParams = socketMessage.CommandArgs();
            var rep = 0;
            if (commandParams.Length > 0)
            {
                int.TryParse(commandParams[0], out rep);
            }

            var targetUsers = socketMessage.MentionedUsers;
            if (targetUsers.Count == 0)
            {
                Logger.Debug("[ChangeRepCommand]No users were mentioned");
                return;
            }

            foreach (var user in targetUsers)
            {
                DiscordDatabase.SetReputation(user.Id, rep);
            }

            await socketMessage.DeleteAsync();
        }
    }
}
