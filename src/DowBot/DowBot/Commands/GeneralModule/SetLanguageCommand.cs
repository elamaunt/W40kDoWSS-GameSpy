using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Commands.Primitives;
using DiscordBot.Database;

namespace DiscordBot.Commands.GeneralModule
{
    public class SetLanguageCommand: DmCommand, ICommandDescription
    {
        public SetLanguageCommand(DmCommandParams commandParams) : base(commandParams)
        {
            
        }

        public override async Task Execute(SocketMessage socketMessage, bool isRus, CommandAccessLevel accessLevel)
        {
            var commandParams = socketMessage.CommandArgs();

            if (commandParams.Length <= 0)
                return;

            var lang = commandParams[0];
            
            if (lang == "ru")
            {
                BotDatabase.SetUserLanguage(socketMessage.Author.Id, true);
                await socketMessage.Channel.SendMessageAsync(":flag_ru: Русский язык был успешно установлен! :flag_ru:");
            }
            else if (lang == "en")
            {
                BotDatabase.SetUserLanguage(socketMessage.Author.Id, false);
                await socketMessage.Channel.SendMessageAsync(":flag_us: English language has been succesfully set! :flag_us:");
            }
        }

        public string RuDescription
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine("Эта команда устанавливает один из двух допустимых языков: ");
                sb.AppendLine("!setlang ru (русский, russian)");
                sb.AppendLine("!setlang en (английский, english)");


                return sb.ToString();
            }
        }
        
        public string EnDescription
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine("This command setting up your language, only two languages are available: ");
                sb.AppendLine("!setlang ru (русский, russian)");
                sb.AppendLine("!setlang en (английский, english)");

                return sb.ToString();
            }
        }
    }
}