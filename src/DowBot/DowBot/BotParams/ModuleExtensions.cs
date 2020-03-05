using DiscordBot.Commands.Primitives;

namespace DiscordBot.BotParams
{
    public static class ModuleExtensions
    {
        public static void OverrideCommandParams(this IModuleDmCommandsParams moduleDmCommandsParams, CommandId commandId, DmCommandParams commandParams)
        {
            moduleDmCommandsParams.DmCommandsParams[commandId] = commandParams;
        }
        
        public static void OverrideCommandParams(this IModuleGuildCommandsParams moduleDmCommandsParams, CommandId commandId, GuildCommandParams commandParams)
        {
            moduleDmCommandsParams.GuildCommandsParams[commandId] = commandParams;
        }
    }
}