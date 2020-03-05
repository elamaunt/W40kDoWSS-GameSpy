using System;
using System.Collections.Generic;

namespace DiscordBot.BotParams
{
    public class BotParams
    {
        public GeneralModuleParams GeneralModuleParams { get; }
        public RandomModuleParams RandomModuleParams { get; }
        public AdministrativeModuleParams AdministrativeModuleParams { get; }
        public DynamicModuleParams DynamicModuleParams { get; }
        public SyncModuleParams SyncModuleParams { get; }
        public CustomCommandsModuleParams CustomCommandsModuleParams { get; }

        public BotParams(IEnumerable<IModuleParams> modules)
        {
            foreach (var module in modules)
            {
                switch (module)
                {
                    case GeneralModuleParams generalGuildModuleParams:
                        GeneralModuleParams = generalGuildModuleParams;
                        break;
                    case RandomModuleParams randomModuleParams:
                        RandomModuleParams = randomModuleParams;
                        break;
                    case AdministrativeModuleParams adminModuleParams:
                        AdministrativeModuleParams = adminModuleParams;
                        break;
                    case DynamicModuleParams dynamicDataModuleParams:
                        DynamicModuleParams = dynamicDataModuleParams;
                        break;
                    case SyncModuleParams syncModule:
                        SyncModuleParams = syncModule;
                        break;
                    case CustomCommandsModuleParams customCommandsModuleParams:
                        CustomCommandsModuleParams = customCommandsModuleParams;
                        break;
                }
            }
            if (GeneralModuleParams == null)
                throw new Exception("You must pass GeneralModuleParams to BotParams constructor!");
        }
        
    }
}