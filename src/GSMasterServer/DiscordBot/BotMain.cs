using Discord;
using Discord.WebSocket;
using GSMasterServer.DiscordBot.Commands;
using GSMasterServer.DiscordBot.Database;
using IrcNet.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Rest;
using SocketGuildUser = Discord.WebSocket.SocketGuildUser;
using SocketUser = Discord.WebSocket.SocketUser;
using SocketUserMessage = Discord.WebSocket.SocketUserMessage;


namespace GSMasterServer.DiscordBot
{
    internal class BotMain
    {
        private static DiscordSocketClient BotClient { get; set; }
        private static SocketGuild ThunderGuild { get; set; }

        private static string[] numbers = new[] { ":one:", ":two:", ":three:", ":four:", ":five:", ":six:", ":seven:", ":eight:", ":nine:", ":one::zero:" };

        private static Random random = new Random();

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
            ThunderGuild = BotClient.GetGuild(DiscordServerConstants.ServerId);
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

                if (((SocketTextChannel) arg.Channel).Guild.Id != DiscordServerConstants.ServerId)
                    return;

                if (BotCommands.CommandStrings.Any(x => arg.Content.StartsWith(x)))
                {
                    await BotCommands.HandleCommand(arg);
                }
                else
                {
                    await ActivityBonus(arg);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private static async Task ActivityBonus(SocketMessage arg)
        {
            if (random.Next(0, 250) == 0)
            {
                var user = arg.Author;
                DiscordDatabase.SetReputation(user.Id, DiscordDatabase.GetProfile(user.Id).Reputation + 1);
                var guidUser = user as SocketGuildUser;
                var text = $"#activityrep {guidUser?.Nickname ?? user.Username} is getting +1 rep for activity!!";
                await WriteToLogChannel(text);
                await UpdateRepTop();
            }
        }

        private static Task LogAsync(LogMessage arg)
        {
            Logger.Debug(arg);
            return Task.CompletedTask;
        }

        public static async Task WriteToLogChannel(string text)
        {
            var channel = ThunderGuild.GetTextChannel(DiscordServerConstants.RepLogChannelId);
            if (channel == null)
            {
                Logger.Warn("Tried to write log to channel, but the channel is null!");
                return;
            }
            await channel.SendMessageAsync(text);
        }

        public static async Task UpdateRepTop()
        {
            var repTopText = new StringBuilder();
            var topByRating = DiscordDatabase.GetTopByRating();
            repTopText.AppendLine("➡️ REPUTATION TOP");
            var userTasks = topByRating.Select(topUser => BotClient.Rest.GetUserAsync(topUser.UserId)).ToList();

            var restUsers = await Task.WhenAll(userTasks);

            var iter = 0;
            foreach (var topUser in topByRating)
            {
                var user = restUsers.FirstOrDefault(x => x.Id == topUser.UserId);
                if (user == null)
                    continue;
                //var guildUser = user as RestGuildUser;
                var nickName = user.Mention;
                repTopText.AppendLine($"{numbers[iter++]} {nickName} **{topUser.Reputation}** ({BotExtensions.RepName(topUser.Reputation)})");
            }
            var channel = ThunderGuild.GetTextChannel(DiscordServerConstants.RepTopChannelId);
            var messages = await channel.GetMessagesAsync(1).FlattenAsync();
            var message = messages.FirstOrDefault(x => x.Author.Id == BotClient.CurrentUser.Id);
            if (message != null && message is RestUserMessage socketMessage)
            {
                await socketMessage.ModifyAsync(x => x.Content = repTopText.ToString());
            }
            else
            {
                await channel.SendMessageAsync(repTopText.ToString());
            }
        }

    }
}
