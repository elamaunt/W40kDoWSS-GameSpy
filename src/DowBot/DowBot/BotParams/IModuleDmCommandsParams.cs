using System.Collections.Generic;
using DiscordBot.Commands.Primitives;

namespace DiscordBot.BotParams
{
    public interface IModuleDmCommandsParams
    {
        Dictionary<CommandId, DmCommandParams> DmCommandsParams { get; }
    }
}