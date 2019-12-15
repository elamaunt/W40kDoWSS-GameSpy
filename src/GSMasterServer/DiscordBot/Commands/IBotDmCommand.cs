using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;

namespace GSMasterServer.DiscordBot.Commands
{
    public interface IBotDmCommand
    {
        Task Execute(SocketMessage socketMessage, IGuild thunderGuild);

        AccessLevel MinAccessLevel { get; }
    }
}
