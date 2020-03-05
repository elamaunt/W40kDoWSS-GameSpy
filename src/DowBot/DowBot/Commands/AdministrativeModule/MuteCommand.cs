using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Commands.Primitives;

namespace DiscordBot.Commands.AdministrativeModule
{
    internal class MuteCommand: GuildCommand
    {
        private readonly AdminCommandsManager _adminManager;
        
        public MuteCommand(AdminCommandsManager adminManager, GuildCommandParams guildCommandParams) : base(guildCommandParams)
        {
            _adminManager = adminManager;
        }

        // Usage: <how-long in minutes> <list of @mentions>
        public override async Task Execute(SocketMessage socketMessage)
        {
            var commandParams = socketMessage.CommandArgs();
            var paramCount = commandParams.Length;
            ulong howLong = 0;
            if (paramCount > 0)
            {
                ulong.TryParse(commandParams[0], out howLong);
            }
            var targetUsers = socketMessage.MentionedUsers;
            if (targetUsers.Count == 0)
            {
                DowBotLogger.Trace("[MuteCommand]No users were mentioned");
                return;
            }

            List<SocketUser> mutedUsers;
            if (howLong != 0)
            {
                var timeUntilMute = DateTime.UtcNow.AddMinutes(howLong).Ticks;
                mutedUsers = await _adminManager.MuteAsync(targetUsers, timeUntilMute);
            }
            else
            {
                mutedUsers = await _adminManager.MuteAsync(targetUsers, -1);
            }

            var respMessage = new StringBuilder();
            respMessage.Append("Successfully muted: ");
            foreach (var user in mutedUsers)
            {
                respMessage.AppendLine($"<@{user.Id}>");
                try
                {
                    var logMessage = new StringBuilder();
                    logMessage.Append(howLong != 0
                        ? $"You've been muted for {howLong} minutes"
                        : $"You've been muted FOREVER!");
                    var channelToWrite = await user.GetOrCreateDMChannelAsync();
                    await channelToWrite.SendMessageAsync(logMessage.ToString());
                }
                catch {}
            }
            
            await socketMessage.DeleteAsync();
            var respChannel = await socketMessage.Author.GetOrCreateDMChannelAsync();
            await respChannel.SendMessageAsync(respMessage.ToString());
        }
        
    }
}