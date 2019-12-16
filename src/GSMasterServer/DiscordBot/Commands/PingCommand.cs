using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;

namespace GSMasterServer.DiscordBot.Commands
{
    public class PingCommand : IBotDmCommand
    {
        public AccessLevel MinAccessLevel { get; } = AccessLevel.Moderator;
        public async Task Execute(SocketMessage socketMessage, IGuild thunderGuild)
        {
            await socketMessage.Channel.SendMessageAsync("pong!");
        }
    }
}
