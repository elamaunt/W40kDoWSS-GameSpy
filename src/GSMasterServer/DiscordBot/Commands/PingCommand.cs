using Discord.WebSocket;
using System.Threading.Tasks;

namespace GSMasterServer.DiscordBot.Commands
{
    public class PingCommand : IBotCommand
    {
        public AccessLevel MinAccessLevel { get; } = AccessLevel.Moderator;
        public async Task Execute(string[] commandParams, SocketMessage socketMessage)
        {
            await socketMessage.Channel.SendMessageAsync("pong!");
        }
    }
}
