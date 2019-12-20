using Discord.WebSocket;
using System.Threading.Tasks;

namespace GSMasterServer.DiscordBot.Commands
{
    internal class PingCommand : IBotDmCommand
    {
        public AccessLevel MinAccessLevel { get; } = AccessLevel.Moderator;
        public async Task Execute(SocketMessage socketMessage, BotManager botManager, AccessLevel accessLevel)
        {
            await socketMessage.Channel.SendMessageAsync("pong!");
        }
    }
}
