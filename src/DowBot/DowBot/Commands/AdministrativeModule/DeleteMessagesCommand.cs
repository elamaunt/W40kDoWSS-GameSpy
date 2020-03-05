using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBot.Commands.Primitives;

namespace DiscordBot.Commands.AdministrativeModule
{
    internal class DeleteMessagesCommand: GuildCommand
    {
        private readonly AdminCommandsManager _adminManager;
        
        public DeleteMessagesCommand(AdminCommandsManager adminManager, GuildCommandParams guildCommandParams) : base(guildCommandParams)
        {
            _adminManager = adminManager;
        }

        // Usage: <messages count OR fromMessage ID> <list of @mentions>
        public override async Task Execute(SocketMessage socketMessage)
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
                DowBotLogger.Trace("[DeleteMessagesCommand]No required arg was passed");
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