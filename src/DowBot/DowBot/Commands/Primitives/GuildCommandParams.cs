using System;

namespace DiscordBot.Commands.Primitives
{
    /// <summary>
    /// User can manually specify these parameters!
    /// </summary>
    public class GuildCommandParams
    {
        /// <summary>
        /// You can manually specify usageids for UsageArea channels or categories
        /// </summary>
        public readonly CommandAccessLevel AccessLevel;
        public readonly CommandUsageArea UsageArea;
        public readonly CommandUsageAreaType UsageAreaType;
        
        public ulong[] UsageIds;

        public GuildCommandParams(CommandAccessLevel accessLevel, CommandUsageArea usageArea, 
            CommandUsageAreaType usageAreaType, ulong[] usageIds = null)
        {
            AccessLevel = accessLevel;
            UsageArea = usageArea;
            UsageAreaType = usageAreaType;
            UsageIds = usageIds;
        }
    }
}