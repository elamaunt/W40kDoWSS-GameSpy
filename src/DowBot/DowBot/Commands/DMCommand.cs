using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Commands.Primitives;

namespace DiscordBot.Commands
{
    public abstract class DmCommand
    {
        public readonly DmCommandParams Params;

        protected DmCommand(DmCommandParams commandParams)
        {
            Params = commandParams;
        }
        public abstract Task Execute(SocketMessage socketMessage, bool isRus, CommandAccessLevel accessLevel);
    }
}