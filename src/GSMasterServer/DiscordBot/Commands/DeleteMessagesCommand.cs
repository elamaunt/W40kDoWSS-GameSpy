using Discord;
using Discord.WebSocket;
using IrcNet.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSMasterServer.DiscordBot.Commands
{
    internal class DeleteMessagesCommand : IBotCommand
    {
        public AccessLevel MinAccessLevel { get; } = AccessLevel.Moderator;

        public AllowLevel AllowLevel { get; } = AllowLevel.Everywhere;

        public async Task Execute(SocketMessage socketMessage)
        {
            var commandParams = socketMessage.CommandArgs();
            var paramCount = commandParams.Length;
            ushort messagesCount = 0;
            ulong fromMessage = 0;
            if (paramCount > 0)
            {
                if (!ushort.TryParse(commandParams[0], out messagesCount))
                    ulong.TryParse(commandParams[0], out fromMessage);
            }

            if (messagesCount == 0 && fromMessage == 0)
            {
                Logger.Trace("[DeleteMessagesCommand]No required arg was passed");
                return;
            }

            var targetUsers = socketMessage.MentionedUsers;

            var textChannel = (SocketTextChannel) socketMessage.Channel;
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
