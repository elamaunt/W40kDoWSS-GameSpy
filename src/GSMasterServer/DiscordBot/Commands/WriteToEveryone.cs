using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using IrcNet.Tools;

namespace GSMasterServer.DiscordBot.Commands
{
    public class WriteToEveryone : IBotDmCommand
    {
        public AccessLevel MinAccessLevel { get; } = AccessLevel.Admin;

        public async Task Execute(SocketMessage socketMessage, IGuild thunderGuild)
        {
            var skipedText = socketMessage.Content.Split().Skip(1);
            var text = string.Join("", skipedText);
            var users = await thunderGuild.GetUsersAsync();
            foreach (var user in users)
            {
                try
                {
                    if (user.IsBot)
                        continue;
                    if (user.Id == socketMessage.Author.Id)
                        continue;
                    var dm = await user.GetOrCreateDMChannelAsync();
                    await dm.SendMessageAsync(text);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex);
                }
            }

            await socketMessage.Channel.SendMessageAsync("Success!");
        }
    }
}
