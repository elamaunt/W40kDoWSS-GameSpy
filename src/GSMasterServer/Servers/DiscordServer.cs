using Discord;
using Discord.WebSocket;
using GSMasterServer.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GSMasterServer.Servers
{
    internal class DiscordServer
    {
        private const long serverId = 606832876369215491;
        private const long welcomeChannelId = 606832876369215495;

        private readonly DiscordSocketClient client;
        public DiscordServer()
        {
            try
            {
                client = new DiscordSocketClient();

                client.Log += LogAsync;
                client.Ready += ReadyAsync;
                client.MessageReceived += MessageReceivedAsync;

                StartAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private async Task StartAsync()
        {
            await client.LoginAsync(TokenType.Bot, GetToken());
            await client.StartAsync();
        }

        private string GetToken()
        {
            var token = File.ReadAllText("discord_token.txt");
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Token is empty!");
            return token;
        }

        private async Task MessageReceivedAsync(SocketMessage arg)
        {
            if (arg.Author.Id == client.CurrentUser.Id)
                return;
            return;
        }

        private Task ReadyAsync()
        {
            Logger.Info($"{client} is ready!");
            return Task.CompletedTask;
        }

        private Task LogAsync(LogMessage arg)
        {
            Logger.Info(arg);
            return Task.CompletedTask;
        }

    }
}
