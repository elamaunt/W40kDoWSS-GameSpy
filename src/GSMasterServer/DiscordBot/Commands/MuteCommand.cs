using Discord;
using Discord.WebSocket;
using GSMasterServer.DiscordBot.Database;
using IrcNet.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSMasterServer.DiscordBot.Commands
{
    public class MuteCommand : IBotCommand
    {
        public AccessLevel MinAccessLevel { get; } = AccessLevel.Moderator;

        private readonly bool softMute = true;
        public MuteCommand(bool softMute)
        {
            this.softMute = softMute;
        }

        /// <summary>
        /// Possible command params:
        /// (optionally) How long (ulong minutes)
        /// If not passed, user will be muted forever
        /// </summary>
        /// <param name="commandParams"></param>
        /// <param name="socketMessage"></param>
        /// <returns></returns>
        public async Task Execute(SocketMessage socketMessage)
        {
            string[] commandParams = socketMessage.CommandArgs();
            var paramCount = commandParams.Length;
            ulong howLong = 0;
            if (paramCount > 0)
            {
                ulong.TryParse(commandParams[0], out howLong);
            }
            var targetUsers = socketMessage.MentionedUsers;
            if (targetUsers.Count == 0)
                throw new Exception("[MuteCommand]No user were mentioned");

            var server = (socketMessage.Channel as SocketGuildChannel).Guild;
            ulong roleId = softMute ? DiscordServerConstants.floodOnlyRoleId : DiscordServerConstants.readOnlyRoleId;
            foreach (var user in targetUsers)
            {
                var guidUser = user as SocketGuildUser;
                await guidUser.AddRoleAsync(server.GetRole(roleId));
                if (howLong != 0)
                {
                    var timeUntilMute = (ulong)DateTime.UtcNow.AddMinutes(howLong).Ticks;
                    DiscordDatabase.AddMute(guidUser.Id, softMute ? timeUntilMute : 0, !softMute ? timeUntilMute : 0);
                }
            }

            var logMessage = new StringBuilder();
            foreach (var user in targetUsers)
            {
                logMessage.Append($"<@{user.Id}> ");
            }
            if (howLong != 0)
                logMessage.Append($"You've been {(softMute ?  "soft-" : "")}muted for {howLong} minutes");
            else
                logMessage.Append($"You've been {(softMute ? "soft-" : "")}muted FOREVER!");
            await BotMain.WriteToLogChannel(logMessage.ToString());

            await socketMessage.DeleteAsync();
        }
    }
}
