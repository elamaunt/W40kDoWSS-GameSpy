using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Commands;
using DiscordBot.Commands.Primitives;

namespace BotLauncher
{
    public class GetpCommand: DmCommand
    {
        public GetpCommand(DmCommandParams commandParams) : base(commandParams)
        {
            
        }

        public override async Task Execute(SocketMessage socketMessage, CommandAccessLevel accessLevel)
        {
            await socketMessage.Channel.SendMessageAsync("Here is custom command!");
        }
    }
}