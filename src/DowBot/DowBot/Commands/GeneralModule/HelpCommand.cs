using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Commands.Primitives;

namespace DiscordBot.Commands.GeneralModule
{
    internal class HelpCommand: DmCommand
    {
        private readonly GuildCommandsHandler _guildCommandsHandler;
        private readonly DmCommandsHandler _dmCommandsHandler;
        
        public HelpCommand(DmCommandParams dmCommandParams, GuildCommandsHandler guildCommandsHandler, DmCommandsHandler dmCommandsHandler) : base(dmCommandParams)
        {
            _guildCommandsHandler = guildCommandsHandler;
            _dmCommandsHandler = dmCommandsHandler;
        }
        
        public override async Task Execute(SocketMessage socketMessage, bool isRus, CommandAccessLevel accessLevel)
        {
            var commandParams = socketMessage.CommandArgs();

            var lang = isRus;
            
            var sb = new StringBuilder();
            if (commandParams.Length <= 0)
            {                   
                sb.AppendLine($"=== **DoWExpert Bot v. {Constants.Version}** === ");
                sb.AppendLine(lang ? ":flag_ru: Ваш язык - русский! :flag_ru:  (Use **!setlang en** to set English!)" : ":flag_us: Your language is English! :flag_us:  (Используйте **!setlang ru** для установки русского!)");
                sb.AppendLine();
                sb.Append(lang ? "Вы можете использовать следующие серверные команды: " : "You are allowed to use these server commands: ");

                var b = false;
                foreach (var cmd in _guildCommandsHandler.GetCommands(accessLevel))
                {
                    if (b)
                        sb.Append(", ");
                    sb.Append("**!" + cmd + "**");
                    b = true;
                }

                b = false;
                sb.Append(lang ? "И эти приватные команды: " : "\nAnd these direct commands: ");
                foreach (var cmd in _dmCommandsHandler.GetCommands(accessLevel))
                {
                    if (b)
                        sb.Append(", ");
                    sb.Append("**!" + cmd + "**");
                    b = true;
                }
            
                
                sb.AppendLine(lang ? "\nВы можете получить доп. информацию по команде используя **!help + [название команды]**" : 
                    "\nYou can get extra information for each command by using: **!help + [command name]**");

                sb.AppendLine(lang ? "\n\nЭтот бот был создан: <@405828339908214796>" : "\n\nThis bot was created by <@405828339908214796>");
            }
            else
            {
                var cmdText = commandParams[0];
                var gc = _guildCommandsHandler.GetCommand(cmdText);
                var dc = _dmCommandsHandler.GetCommand(cmdText);

                if (gc == null && dc == null)
                {
                    sb.AppendLine(lang ? "Такой команды не существует!" :"The command you ask does not exist!");
                }
                else
                {
                    if (gc != null)
                    {
                        sb.AppendLine(lang ? $"Информация о серверной команде: **{cmdText}**" : $"Information about server command: **{cmdText}**");
                        if (gc is ICommandDescription commandDescription)
                        {
                            var cmdDescription = lang ? commandDescription.RuDescription : commandDescription.EnDescription;
                            sb.AppendLine($"```{cmdDescription}```");
                        }
                        else
                        {
                            sb.AppendLine(lang ? $"Извините, но эта команда не имеет описания! **{cmdText}**" : "Sorry, but this command does not has description yet!");
                        }
                    }
                    else
                    {
                        sb.AppendLine(lang ? $"Информация о приватной команде: **{cmdText}**" : $"Information about dm command: **{cmdText}**");
                        if (dc is ICommandDescription commandDescription)
                        {
                            var cmdDescription = lang ? commandDescription.RuDescription : commandDescription.EnDescription;
                            sb.AppendLine($"```{cmdDescription}```");
                        }
                        else
                        {
                            sb.AppendLine(lang ? $"Извините, но эта команда не имеет описания! **{cmdText}**" : "Sorry, but this command does not has description yet!");
                        }
                    }
                }


                
                
                
            }

            await socketMessage.Channel.SendMessageAsync(sb.ToString());
        }
        
    }
}