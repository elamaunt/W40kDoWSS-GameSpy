using Discord;
using Discord.WebSocket;
using IrcNet.Tools;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GSMasterServer.DiscordBot.Commands
{
    public class DeleteMessagesCommand : IBotCommand
    {
        public AccessLevel MinAccessLevel { get; } = AccessLevel.Moderator;
        /// <summary>
        /// Possible command params:
        /// (required) From message(ulong messageId)
        /// (optionally) Delete admin messages (bool) OR from user (ulong userId)
        /// If not passed, no delete admins messages and delete from all users
        /// </summary>
        /// <param name="commandParams"></param>
        /// <param name="socketMessage"></param>
        /// <returns></returns>
        public async Task Execute(SocketMessage socketMessage)
        {
            string[] commandParams = socketMessage.CommandArgs();
            var paramCount = commandParams.Length;
            ulong fromMessage = 0;
            bool deleteAdmin = false;
            ulong fromUser = 0;
            if (paramCount > 0)
            {
                ulong.TryParse(commandParams[0], out fromMessage);
                if (paramCount > 1)
                {
                    if (!bool.TryParse(commandParams[1], out deleteAdmin))
                        ulong.TryParse(commandParams[1], out fromUser);
                }
            }
            if (fromMessage == 0)
                throw new Exception("[DeleteMessagesCommand]No message id was passed");
            var textChannel = socketMessage.Channel as SocketTextChannel;
            var messages = await textChannel.GetMessagesAsync(fromMessage, Direction.After).FlattenAsync();
            var messagesToDelete = messages;
            if (!deleteAdmin)
            {
                if (fromUser != 0)
                {
                    messagesToDelete = messages.Where(m => m.Author.Id == fromUser);
                }
                else
                {
                    messagesToDelete = messages.Where(m => m.Author.GetAccessLevel() < AccessLevel.Admin);
                }
            }
            await textChannel.DeleteMessagesAsync(messagesToDelete);
        }
    }
}
