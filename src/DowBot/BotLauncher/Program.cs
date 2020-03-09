using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        static async Task Main(string[] args)
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
            
            await dowBot.Start();
            //await dowBot.SendSyncMessage("Me", "Hello!!!");
            /*while (true)
            {
                await dowBot.SendSyncMessage("Me", "Hello!!!");
                await Task.Delay(10);
            }*/
            Console.ReadKey();
            await dowBot.Start();
            Console.ReadKey();
            await dowBot.Destroy();
        }

        private static void DowBotOnOnSyncMessageReceived(object sender, SyncEventArgs e)
        {
            Console.WriteLine(e.Text);
        }
    }
}