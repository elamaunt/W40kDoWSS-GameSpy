using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using GSMasterServer.DiscordBot.Database;
using IrcNet.Tools;

namespace GSMasterServer.DiscordBot.Commands
{
    class ProfileInfoCommand : IBotCommand
    {
        public AccessLevel MinAccessLevel { get; } = AccessLevel.User;
        public async Task Execute(SocketMessage socketMessage)
        {
            var targetUsers = socketMessage.MentionedUsers;
            var user = targetUsers.Count != 1 ? socketMessage.Author : targetUsers.First();

            var profile = DiscordDatabase.GetProfile(user.Id);
            var text =
                $"```\n{user.Username}\nReputation: {BotExtensions.RepName((profile.Reputation))} ({profile.Reputation})```";
            var channelToWrite = await socketMessage.Author.GetOrCreateDMChannelAsync();
            await channelToWrite.SendMessageAsync(text);
            
            await socketMessage.DeleteAsync();
        }
    }
}
