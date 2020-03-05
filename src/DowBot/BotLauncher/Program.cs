using System;
using System.Collections.Generic;
using DiscordBot;
using DiscordBot.BotParams;
using DiscordBot.Commands;
using DiscordBot.Commands.DynamicModule;
using DiscordBot.Commands.Primitives;
using DiscordBot.Commands.SyncModule;

namespace BotLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            var getpCommand = new GetpCommand(new DmCommandParams(CommandAccessLevel.Admin));
            var token = Environment.GetEnvironmentVariable("DowExpertBotToken", EnvironmentVariableTarget.User);
            var botParams = new BotParams(new List<IModuleParams>
            {
                new GeneralModuleParams(token, 680708005783797770, new Logger()),
                new RandomModuleParams( new ulong[]{ 680708005783797774, 680712276717600779 }),
                new AdministrativeModuleParams(680708072196538376, 680708093386162248, 681126703292219443),
                new DynamicModuleParams(new List<IDynamicDataProvider> {new ServerInfoProvider()}),
                new SyncModuleParams(680712313262702624),
                new CustomCommandsModuleParams(null, new Dictionary<string, DmCommand> { {"c1", getpCommand} })
            });
            
            var dowBot = new DowBot(botParams);
            dowBot.OnSyncMessageReceived += DowBotOnOnSyncMessageReceived;
            
            
            dowBot.Start();
            Console.ReadKey();
            dowBot.SendSyncMessage("Me", "Hello!!!");
            dowBot.Stop();
            Console.ReadKey();
            dowBot.Start();
            Console.ReadKey();
            dowBot.Destroy();
        }

        private static void DowBotOnOnSyncMessageReceived(object sender, SyncEventArgs e)
        {
            Console.WriteLine(e.Text);
        }
    }
}