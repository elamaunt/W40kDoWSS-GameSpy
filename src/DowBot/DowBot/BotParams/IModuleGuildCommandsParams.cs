using System.Collections.Generic;
using DiscordBot.Commands.Primitives;

namespace DiscordBot.BotParams
{
    public interface IModuleGuildCommandsParams
    {
        Dictionary<CommandId, GuildCommandParams> GuildCommandsParams { get; }
    }
}