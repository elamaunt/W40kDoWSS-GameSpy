using System.Collections.Generic;
using DiscordBot.Commands;

namespace DiscordBot.BotParams
{
    public class CustomCommandsModuleParams: IModuleParams
    {
        public readonly Dictionary<string, GuildCommand> CustomGuildsCommands;
        public readonly Dictionary<string, DmCommand> CustomDmCommands;

        public CustomCommandsModuleParams(Dictionary<string, GuildCommand> guildCommands, Dictionary<string, DmCommand> dmCommands)
        {
            CustomGuildsCommands = guildCommands;
            CustomDmCommands = dmCommands;
            
            if (guildCommands == null)
                CustomGuildsCommands = new Dictionary<string, GuildCommand>();
            
            if (dmCommands == null)
                CustomDmCommands = new Dictionary<string, DmCommand>();
        }
    }
}