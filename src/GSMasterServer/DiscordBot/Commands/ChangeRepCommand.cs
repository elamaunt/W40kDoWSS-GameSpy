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
    public class ChangeRepCommand : IBotCommand
    {
        public AccessLevel MinAccessLevel { get; } = AccessLevel.User;

        private readonly bool _repAction;
        public ChangeRepCommand(bool repAction)
        {
            _repAction = repAction;
        }
        public async Task Execute(SocketMessage socketMessage)
        {
            var targetUsers = socketMessage.MentionedUsers;
            if (targetUsers.Count != 1)
            {
                Logger.Debug("[ChangeRepCommand]No user was mentioned");
                return;
            }

            var user = targetUsers.First();
            var rep =  DiscordDatabase.ChangeRep(user.Id, socketMessage.Author.Id, _repAction);
            await BotMain.WriteToLogChannel($"<@{user.Id}> your reputation was changed" +
                                            $" {(rep.Item1 > 0 ? "+" : "-")}{Math.Abs(rep.Item1)} ({rep.Item2}) by {socketMessage.Author.Username}");
            await socketMessage.DeleteAsync();
        }
    }
}
