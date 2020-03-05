using System.Collections.Generic;
using DiscordBot.Commands.Primitives;
using DiscordBot.Communication;

namespace DiscordBot.BotParams
{
    public class GeneralModuleParams: IModuleParams, IModuleDmCommandsParams
    {
        public ulong MainGuildId { get; }
        
        public IDowLogger DowLogger { get; }
        
        public string Token { get; }
        
        public Dictionary<CommandId, DmCommandParams> DmCommandsParams { get; } = new Dictionary<CommandId, DmCommandParams>()
        {
            { CommandId.HelpCommand, new DmCommandParams(CommandAccessLevel.Everyone) }
        };

        public GeneralModuleParams(string token, ulong mainGuildId, IDowLogger dowLogger = null)
        {
            Token = token;
            MainGuildId = mainGuildId;
            DowLogger = dowLogger;
        }
    }
}