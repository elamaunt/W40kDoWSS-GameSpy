using Discord;
using Discord.WebSocket;
using GSMasterServer.DiscordBot.Database;
using IrcNet.Tools;
using System;
using System.IO;
using System.Threading.Tasks;


namespace GSMasterServer.DiscordBot
{
    internal class BotMain
    {
        public static DiscordSocketClient BotClient { get; private set; }

        public static async Task StartAsync()
        {
            try
            {
                BotClient = new DiscordSocketClient();

                BotClient.Log += LogAsync;
                BotClient.Ready += ReadyAsync;
                BotClient.MessageReceived += MessageReceivedAsync;

                await BotClient.LoginAsync(TokenType.Bot, GetToken());
                await BotClient.StartAsync();
                Logger.Info("Discord bot intialized!");

                DiscordDatabase.InitDb();

                //await Task.Run(() => UpdateLoop());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private static string GetToken()
        {
            var token = File.ReadAllText("discord_token.txt");
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Token is empty!");
            return token;
        }

        private static async Task UpdateLoop()
        {
            while (true)
            {  
                await Task.Delay(10000);
            }
        }

        private static async Task MessageReceivedAsync(SocketMessage arg)
        {
            try
            {
                if (arg.Author.Id == BotClient.CurrentUser.Id)
                    return;

                if (arg.Content.StartsWith("!"))
                {
                    await BotCommands.HandleCommand(arg);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private static Task ReadyAsync()
        {
            Logger.Info($"{BotClient} is ready!");
            return Task.CompletedTask;
        }

        private static Task LogAsync(LogMessage arg)
        {
            Logger.Debug(arg);
            return Task.CompletedTask;
        }

    }
}
