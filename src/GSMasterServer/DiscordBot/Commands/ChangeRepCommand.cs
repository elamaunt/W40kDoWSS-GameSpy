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
            if (user.Id == socketMessage.Author.Id)
            {
                var channelToWrite = await socketMessage.Author.GetOrCreateDMChannelAsync();
                await channelToWrite.SendMessageAsync("You can't change your own reputation!\nВы не можете изменять репутацию самому себе!");
                await socketMessage.DeleteAsync();
                return;
            }

            var (repChange, repTotal) = DiscordDatabase.ChangeRep(user.Id, socketMessage.Author.Id, _repAction);
            if (repChange == int.MinValue)
            {
                var channelToWrite = await socketMessage.Author.GetOrCreateDMChannelAsync();
                await channelToWrite.SendMessageAsync("You need to wait 24 hours before you can change the reputation of this user!\n" +
                                                      "Вам необходимо подождать 24 часа прежде чем вы сможете изменить репутацию этого пользователя!");
                await socketMessage.DeleteAsync();
                return;
            }

            await BotMain.WriteToLogChannel($"#rep {user.Username} reputation was changed" +
                                            $" {(repChange > 0 ? "+" : "-")}{Math.Abs(repChange)} ({repTotal}) by {socketMessage.Author.Username}");
            await socketMessage.DeleteAsync();
        }
    }
}
