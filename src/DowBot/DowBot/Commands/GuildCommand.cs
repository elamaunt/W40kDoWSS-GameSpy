using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Commands.Primitives;

namespace DiscordBot.Commands
{
    public abstract class GuildCommand
    {
        public readonly GuildCommandParams Params;

        protected GuildCommand(GuildCommandParams guildCommandParams)
        {
            Params = guildCommandParams;
        }
        public abstract Task Execute(SocketMessage socketMessage);
    }
}