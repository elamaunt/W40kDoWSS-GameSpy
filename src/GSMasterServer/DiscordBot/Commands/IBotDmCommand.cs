using Discord.WebSocket;
using System.Threading.Tasks;

namespace GSMasterServer.DiscordBot.Commands
{
    public interface IBotDmCommand
    {
        Task Execute(SocketMessage socketMessage, BotManager botManager, AccessLevel accessLevel);

        AccessLevel MinAccessLevel { get; }
    }
}
