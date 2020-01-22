using Discord.WebSocket;
using System.Threading.Tasks;

namespace GSMasterServer.DiscordBot.Commands
{
    public interface IBotCommand
    {
        Task Execute(SocketMessage socketMessage);

        AccessLevel MinAccessLevel { get; }

        AllowLevel AllowLevel { get; }
    }
}
