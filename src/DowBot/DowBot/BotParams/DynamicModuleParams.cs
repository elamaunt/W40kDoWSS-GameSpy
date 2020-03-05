using System.Collections.Generic;
using DiscordBot.Commands.DynamicModule;

namespace DiscordBot.BotParams
{
    public class DynamicModuleParams: IModuleParams
    {
        public readonly IEnumerable<IDynamicDataProvider> DataProviders;
        
        public DynamicModuleParams(IEnumerable<IDynamicDataProvider> dataProviders)
        {
            DataProviders = dataProviders;
        }
    }
}