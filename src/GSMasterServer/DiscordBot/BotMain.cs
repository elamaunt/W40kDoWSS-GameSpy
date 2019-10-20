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
        private static SocketGuild ThunderGuild { get; set; }

        public static async Task StartAsync()
        {
            try
            {
                BotClient = new DiscordSocketClient();

                BotClient.Log += LogAsync;
                BotClient.Ready += ReadyAsync;
                BotClient.UserJoined += UserJoinedAsync;
                BotClient.MessageReceived += MessageReceivedAsync;

                await BotClient.LoginAsync(TokenType.Bot, GetToken());
                await BotClient.StartAsync();
                Logger.Info("Discord bot initialized!");

                DiscordDatabase.InitDb();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private static async Task UserJoinedAsync(SocketGuildUser arg)
        {
            var profile = DiscordDatabase.GetProfile(arg.Id);
            if (profile != null)
            {
                var userToMute = new List<SocketUser>() { arg };
                if (profile.IsSoftMuteActive)
                {
                    await MuteCommand.Mute(userToMute, true, ThunderGuild, profile.SoftMuteUntil);
                }
                if (profile.IsMuteActive)
                {
                    await MuteCommand.Mute(userToMute, false, ThunderGuild, profile.MuteUntil);
                }
            }
        }

        private static async Task ReadyAsync()
        {
            ThunderGuild = BotClient.GetGuild(DiscordServerConstants.serverId);
            Logger.Info($"{BotClient} is ready!");

            await Task.Run(UpdateLoop);
        }

        private static string GetToken()
        {
            try
            {
                var token = File.ReadAllText("discord_token.txt");
                if (string.IsNullOrWhiteSpace(token))
                    throw new Exception("Token is empty! Bot stopped working...");
                return token;
            }
            catch (Exception)
            {
                Logger.Info("discord_token.txt was not found! Bot stopped working...");
                throw;
            }
        }

        private static async Task UpdateLoop()
        {
            while (true)
            {
                try
                {
                    var timeNow = DateTime.UtcNow.Ticks;
                    var softUnmuteList = new List<SocketUser>();
                    var unmuteList = new List<SocketUser>();
                    foreach (var tableUser in DiscordDatabase.ProfilesTable.FindAll())
                    {
                        if (tableUser.IsSoftMuteActive && tableUser.SoftMuteUntil != -1 && timeNow >= tableUser.SoftMuteUntil)
                        {
                            softUnmuteList.Add(ThunderGuild.GetUser(tableUser.UserId));
                        }
                        if (tableUser.IsMuteActive && tableUser.MuteUntil != -1 && timeNow >= tableUser.MuteUntil)
                        {
                            unmuteList.Add(ThunderGuild.GetUser(tableUser.UserId));
                        }
                    }
                    if (softUnmuteList.Count != 0)
                    {
                        await UnMuteCommand.UnMute(softUnmuteList, true, ThunderGuild);
                    }
                    if (unmuteList.Count != 0)
                    {
                        await UnMuteCommand.UnMute(unmuteList, false, ThunderGuild);
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

                if (((SocketTextChannel) arg.Channel).Guild.Id != DiscordServerConstants.serverId)
                    return;

                if (BotCommands.CommandStrings.Any(x => arg.Content.StartsWith(x)))
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
            var channel = ThunderGuild.GetTextChannel(DiscordServerConstants.logChannelId);
            if (channel == null)
            {
                Logger.Warn("Tried to write log to channel, but the channel is null!");
                return;
            }
            await channel.SendMessageAsync(text);
        }

    }
}
