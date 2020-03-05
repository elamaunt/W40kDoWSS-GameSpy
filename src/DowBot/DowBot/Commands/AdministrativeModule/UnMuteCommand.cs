using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Commands.Primitives;

namespace DiscordBot.Commands.AdministrativeModule
{
    internal class UnMuteCommand: GuildCommand
    {
        private readonly AdminCommandsManager _adminManager;
        
        public UnMuteCommand(AdminCommandsManager adminManager, GuildCommandParams guildCommandParams) : base(guildCommandParams)
        {
            _adminManager = adminManager;
        }

        // Usage: <list of @mentions>
        public override async Task Execute(SocketMessage socketMessage)
        {
            var targetUsers = socketMessage.MentionedUsers;
            if (targetUsers.Count == 0)
            {
                DowBotLogger.Trace("[UnMuteCommand]No users were mentioned!");
                return;
            }
            
            var unMutedUsers = await _adminManager.UnMuteAsync(targetUsers);
            
            var respMessage = new StringBuilder();
            respMessage.Append("Successfully unmuted: ");
            foreach (var user in unMutedUsers)
            {
                respMessage.AppendLine($"<@{user.Id}>");
                try
                {
                    var logMessage = new StringBuilder();
                    logMessage.Append("Congrats! You have been unmuted!");
                    var channelToWrite = await user.GetOrCreateDMChannelAsync();
                    await channelToWrite.SendMessageAsync(logMessage.ToString());
                } catch {}
            }

            await socketMessage.DeleteAsync();
            var respChannel = await socketMessage.Author.GetOrCreateDMChannelAsync();
            await respChannel.SendMessageAsync(respMessage.ToString());
        }
        
    }
}