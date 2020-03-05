using System.Collections.Generic;
using DiscordBot.Commands.Primitives;
using RandomTools;

namespace DiscordBot.BotParams
{
    public class RandomModuleParams: IModuleParams, IModuleGuildCommandsParams
    {
        public readonly IDowItemsProvider DowItemsProvider;
        
        public Dictionary<CommandId, GuildCommandParams> GuildCommandsParams { get; } = new Dictionary<CommandId, GuildCommandParams>()
        {
            { CommandId.RandomCommand, new GuildCommandParams(CommandAccessLevel.Everyone, CommandUsageArea.Channel, CommandUsageAreaType.Allow) },
            { CommandId.ShuffleCommand, new GuildCommandParams(CommandAccessLevel.Everyone, CommandUsageArea.Channel, CommandUsageAreaType.Allow) },
        };
        
        public RandomModuleParams(ulong[] randomChannelsIds = null, IDowItemsProvider dowItemsProvider = null)
        {
            GuildCommandsParams[CommandId.RandomCommand].UsageIds = randomChannelsIds;
            DowItemsProvider = dowItemsProvider;
        }
        
    }
}