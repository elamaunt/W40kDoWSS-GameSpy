using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Commands.Primitives;

namespace DiscordBot.Commands.AdministrativeModule
{
    internal class UnBanCommand: GuildCommand
    {
        private readonly AdminCommandsManager _adminManager;
        
        public UnBanCommand(AdminCommandsManager adminManager, GuildCommandParams guildCommandParams) : base(guildCommandParams)
        {
            _adminManager = adminManager;
        }

        // Usage: list of @mentions>
        public override async Task Execute(SocketMessage socketMessage, bool isRus)
        {
            var targetUsers = socketMessage.MentionedUsers;
            if (targetUsers.Count == 0)
            {
                DowBotLogger.Trace("[UnBanCommand]No users were mentioned");
                return;
            }

            var unBannedUsers = await _adminManager.UnBanAsync(targetUsers);
            
            await socketMessage.DeleteAsync();

            if (unBannedUsers.Count <= 0)
                return;
            var respMessage = new StringBuilder();
            respMessage.Append("Successfully unbanned: ");
            foreach (var user in unBannedUsers)
            {
                respMessage.AppendLine($"<@{user.Id}>");
            }
            var respChannel = await socketMessage.Author.GetOrCreateDMChannelAsync();
            await respChannel.SendMessageAsync(respMessage.ToString());
        }
        
    }
}