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
    public class DeleteMessagesCommand : IBotCommand
    {
        public AccessLevel MinAccessLevel { get; } = AccessLevel.Moderator;

        /// <summary>
        /// Possible command params:
        /// (required) From message(ulong messageId) or messages count(ushort)
        /// If not passed, no delete admins messages and delete from all users
        /// </summary>
        /// <param name="socketMessage"></param>
        /// <returns></returns>
        public async Task Execute(SocketMessage socketMessage)
        {
            string[] commandParams = socketMessage.CommandArgs();
            var paramCount = commandParams.Length;
            ushort messagesCount = 0;
            ulong fromMessage = 0;
            if (paramCount > 0)
            {
                if (!ushort.TryParse(commandParams[0], out messagesCount))
                    ulong.TryParse(commandParams[0], out fromMessage);
            }
            if (messagesCount == 0 && fromMessage == 0)
                Logger.Debug("[DeleteMessagesCommand]No required arg was passed");

            var targetUsers = socketMessage.MentionedUsers;

            var textChannel = socketMessage.Channel as SocketTextChannel;
            IEnumerable<IMessage> messages;
            if (messagesCount == 0)
                messages = await textChannel.GetMessagesAsync(fromMessage, Direction.After, limit: 10000).FlattenAsync();
            else
                messages = await textChannel.GetMessagesAsync(messagesCount + 1).FlattenAsync();

            if (targetUsers.Count != 0)
                messages = messages.Where(m => targetUsers.Contains(m.Author));

            await textChannel.DeleteMessagesAsync(messages);
        }
    }
}
