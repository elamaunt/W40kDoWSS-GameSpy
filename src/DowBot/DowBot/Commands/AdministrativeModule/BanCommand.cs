using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Commands.Primitives;

namespace DiscordBot.Commands.AdministrativeModule
{
    internal class BanCommand: GuildCommand
    {
        private readonly AdminCommandsManager _adminManager;
        
        public BanCommand(AdminCommandsManager adminManager, GuildCommandParams guildCommandParams) : base(guildCommandParams)
        {
            _adminManager = adminManager;
        }

        // Usage: <how-long in days> <list of @mentions>
        public override async Task Execute(SocketMessage socketMessage, bool isRus)
        {
            var commandParams = socketMessage.CommandArgs();
            var paramCount = commandParams.Length;
            sbyte howLong = 0;
            if (paramCount > 0)
            {
                sbyte.TryParse(commandParams[0], out howLong);
            }

            if (howLong < 0 || howLong > 7)
                howLong = 0;
            
            var targetUsers = socketMessage.MentionedUsers;
            if (targetUsers.Count == 0)
            {
                DowBotLogger.Trace("[UnBanCommand]No users were mentioned");
                return;
            }

            var bannedUsers = await _adminManager.BanAsync(targetUsers, howLong);


            await socketMessage.DeleteAsync();

            if (bannedUsers.Count <= 0)
                return;
            var respMessage = new StringBuilder();
            respMessage.Append("Successfully banned: ");
            foreach (var user in bannedUsers)
            {
                respMessage.AppendLine($"<@{user.Id}>");
            }
            var respChannel = await socketMessage.Author.GetOrCreateDMChannelAsync();
            await respChannel.SendMessageAsync(respMessage.ToString());
        }
        
    }
}