using Discord;
using Discord.WebSocket;
using GSMasterServer.DiscordBot.Commands;
using GSMasterServer.DiscordBot.Database;
using IrcNet.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace GSMasterServer.DiscordBot
{
    internal class BotMain
    {
        private static DiscordSocketClient BotClient { get; set; }
        private static SocketGuild SocketGuild { get; set; }

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
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private static async Task ReadyAsync()
        {
            SocketGuild = BotClient.GetGuild(DiscordServerConstants.serverId);
            Logger.Info($"{BotClient} is ready!");

            await Task.Run(() => UpdateLoop());
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
                try
                {
                    var timeNow = (ulong)DateTime.UtcNow.Ticks;
                    var softUnmuteList = new List<SocketUser>();
                    var unmuteList = new List<SocketUser>();
                    foreach (var tableUser in DiscordDatabase.ProfilesTable.FindAll())
                    {
                        if (tableUser.IsSoftMuteActive && timeNow >= tableUser.SoftMuteUntil)
                        {
                            softUnmuteList.Add(SocketGuild.GetUser(tableUser.UserId));
                            DiscordDatabase.RemoveMute(tableUser.UserId, true);
                        }
                        if (tableUser.IsMuteActive && timeNow >= tableUser.MuteUntil)
                        {
                            unmuteList.Add(SocketGuild.GetUser(tableUser.UserId));
                            DiscordDatabase.RemoveMute(tableUser.UserId, false);
                        }
                    }
                    if (softUnmuteList.Count != 0)
                    {
                        await UnMuteCommand.UnMute(softUnmuteList, true, SocketGuild);
                    }
                    if (unmuteList.Count != 0)
                    {
                        await UnMuteCommand.UnMute(unmuteList, false, SocketGuild);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
                finally
                {
                    await Task.Delay(10000);
                }
            }
        }

        private static async Task MessageReceivedAsync(SocketMessage arg)
        {
            try
            {
                if (arg.Author.Id == BotClient.CurrentUser.Id)
                    return;

                if ((arg.Channel as SocketTextChannel).Guild.Id != DiscordServerConstants.serverId)
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

        private static Task LogAsync(LogMessage arg)
        {
            Logger.Debug(arg);
            return Task.CompletedTask;
        }

        public static async Task WriteToLogChannel(string text)
        {
            var channel = SocketGuild.GetTextChannel(DiscordServerConstants.logChannelId);
            if (channel == null)
            {
                Logger.Warn("Tried to write log to channel, but the channel is null!");
                return;
            }
            var socketChannel = channel as SocketTextChannel;
            await socketChannel.SendMessageAsync(text);
        }

    }
}
