using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Commands.Primitives;

namespace DiscordBot.Commands.GeneralModule
{
    internal class HelpCommand: DmCommand
    {
        public HelpCommand(DmCommandParams dmCommandParams) : base(dmCommandParams)
        {
            
        }
        
        public override async Task Execute(SocketMessage socketMessage, CommandAccessLevel accessLevel)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DoWExpert Bot v. {Constants.Version}");
            sb.AppendLine($"This bot was created by <@405828339908214796>");

            await socketMessage.Channel.SendMessageAsync(sb.ToString());
        }
        
    }
}