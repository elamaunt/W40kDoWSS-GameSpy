using Discord.WebSocket;
using System.Threading.Tasks;

namespace GSMasterServer.DiscordBot.Commands
{
    public interface IBotCommand
    {
        Task Execute(string[] commandParams, SocketMessage socketMessage);

        AccessLevel MinAccessLevel { get; }
    }

    public enum AccessLevel : byte
    {
        User = 0,
        Moderator = 1,
        Admin = 2
    }
}
