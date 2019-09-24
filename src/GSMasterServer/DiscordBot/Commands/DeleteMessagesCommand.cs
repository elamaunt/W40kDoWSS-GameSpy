using Discord;
using Discord.WebSocket;
using IrcNet.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSMasterServer.DiscordBot.Commands
{
    public class DeleteMessagesCommand : IBotCommand
    {
        public AccessLevel MinAccessLevel { get; } = AccessLevel.Moderator;

        /// <summary>
        /// Possible command params:
        /// (required) From message(ulong messageId) or messages count(ushort)
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
            ushort messagesCount = 0;
            ulong fromMessage = 0;
            char deleteAdmin = '0';
            ulong fromUser = 0;
            if (paramCount > 0)
            {
                if (!ushort.TryParse(commandParams[0], out messagesCount))
                    ulong.TryParse(commandParams[0], out fromMessage);
                if (paramCount > 1)
                {
                    if (!char.TryParse(commandParams[1], out deleteAdmin))
                        ulong.TryParse(commandParams[1], out fromUser);
                }
            }
            if (messagesCount == 0 && fromMessage == 0)
                throw new Exception("[DeleteMessagesCommand]No required arg was passed");
            var textChannel = socketMessage.Channel as SocketTextChannel;
            IEnumerable<IMessage> messages;
            if (messagesCount == 0)
                messages = await textChannel.GetMessagesAsync(fromMessage, Direction.After, limit: 10000).FlattenAsync();
            else
                messages = await textChannel.GetMessagesAsync(messagesCount).FlattenAsync();
            var messagesToDelete = messages;
            if (deleteAdmin != '1')
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
