using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;

namespace GSMasterServer.DiscordBot.Commands
{
    public interface IBotCommand
    {
        Task Execute(SocketMessage socketMessage);

        AccessLevel MinAccessLevel { get; }
    }
}
