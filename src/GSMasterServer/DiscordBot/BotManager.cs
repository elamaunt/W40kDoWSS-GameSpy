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
using GSMasterServer.Servers;
using SocketGuildUser = Discord.WebSocket.SocketGuildUser;
using SocketUser = Discord.WebSocket.SocketUser;


namespace GSMasterServer.DiscordBot
{
    public class BotManager
    {
        public DiscordSocketClient BotClient { get; }
        public SocketGuild ThunderGuild { get; private set; }
        public IGuild Guild => ThunderGuild;
        public ServerInfoCollector ServerInfoCollector { get; }

        private readonly BotCommandsManager _botCommandsManager;
        private readonly string _token;

        public BotManager(SingleMasterServer singleMasterServer)
        {
            var token = File.ReadAllText("discord_token.txt");
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Token is empty! Bot stopped working...");
            _token = token;

            ServerInfoCollector = new ServerInfoCollector(singleMasterServer, this);
            _botCommandsManager = new BotCommandsManager(this);

            BotClient = new DiscordSocketClient();

            BotClient.Log += LogAsync;
            BotClient.Ready += ReadyAsync;
            BotClient.UserJoined += UserJoinedAsync;
            BotClient.MessageReceived += MessageReceivedAsync;

            DiscordDatabase.InitDb();
            Logger.Info("Discord bot is loaded!");
        }

        public async Task Run()
        {
            try
            {
                await BotClient.LoginAsync(TokenType.Bot, _token);
                await BotClient.StartAsync();
                Logger.Info($"Discord bot is running! {BotClient.CurrentUser}");

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private async Task ReadyAsync()
        {
            ThunderGuild = BotClient.GetGuild(DiscordServerConstants.ServerId);
            Logger.Info($"{BotClient} is ready!");

            await Task.Run(UpdateLoop);
        }

        private async Task UserJoinedAsync(SocketGuildUser arg)
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

        private async Task UpdateLoop()
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

                    await ServerInfoCollector.UpdateServerMessage();
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

        private  async Task MessageReceivedAsync(SocketMessage arg)
        {
            try
            {
                if (arg.Author.Id == BotClient.CurrentUser.Id)
                    return;

                switch (arg.Channel)
                {
                    case SocketDMChannel _:
                    {
                        if (_botCommandsManager.CommandStrings.Any(x => arg.Content.StartsWith(x)))
                        {
                            await _botCommandsManager.HandleDmCommand(arg);
                        }

                        break;
                    }
                    case SocketTextChannel socketTextChannel 
                        when socketTextChannel.Guild.Id == DiscordServerConstants.ServerId &&
                             socketTextChannel.CategoryId == DiscordServerConstants.BotCategoryId:
                    {
                        if (_botCommandsManager.CommandStrings.Any(x => arg.Content.StartsWith(x)))
                        {
                            await _botCommandsManager.HandleCommand(arg);
                        }
                        break;
                    }
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
    }
}
