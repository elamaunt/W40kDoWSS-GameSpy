using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.Commands;
using DiscordBot.Commands.AdministrativeModule;
using DiscordBot.Commands.SyncModule;
using DiscordBot.Database;

namespace DiscordBot
{
    public class DowBot
    {
        private readonly BotParams.BotParams _botParams;
        private readonly GuildCommandsHandler _guildCommands;
        private readonly DmCommandsHandler _dmCommands;
        
        private DiscordSocketClient _socketClient;
        internal SocketGuild MainGuild { get; private set; }
        
        private SocketTextChannel SyncChannel { get; set; }

        private bool _isNormalStop;
        private CancellationTokenSource _cancelStartCheck;
        private CancellationTokenSource _cancelUpdateLoop;
        
        internal readonly AdminCommandsManager AdminCommandsManager;
        
        private const ushort UpdatePeriod = 1000 * 60; // in ms. Now equals to 1 minute.

        internal event EventHandler<SocketGuild> OnBotReady;
        
        #region external
        /// <summary>
        /// Subscribe to this if you want to receive messages from server. Will work only with enabled SyncModuleParams
        /// </summary>
        public event EventHandler<SyncEventArgs> OnSyncMessageReceived;
        public DowBot(BotParams.BotParams botParams)
        {
            if (botParams.GeneralModuleParams.Token == null || string.IsNullOrWhiteSpace(botParams.GeneralModuleParams.Token))
                throw new Exception("Wrong token!");
            
            if (botParams.GeneralModuleParams.DowLogger == null)
                throw new Exception("Logger is null!");
            DowBotLogger.Logger = botParams.GeneralModuleParams.DowLogger;

            if (botParams.AdministrativeModuleParams != null)
                AdminCommandsManager = new AdminCommandsManager(this, botParams);
            
            _guildCommands = new GuildCommandsHandler(this, botParams);
            _dmCommands = new DmCommandsHandler(this, botParams);

            _botParams = botParams;
            
            Initialize();
        }

        public async Task SendSyncMessage(string author, string text)
        {
            if (_botParams.SyncModuleParams == null || SyncChannel == null || _socketClient == null || _socketClient.ConnectionState != ConnectionState.Connected)
                return;

            // это нужно для того, чтобы в Discord не проходили @everyone и @here от тех, у кого нет на это доступа.
            text = text.Replace("@", "");

            SyncChannel.SendMessageAsync($"[{author}] {text}");
        }
        
        /// <summary>
        /// Force start bot and connect it to Discord. on-failure it will be try to connect again. 
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            if (_cancelStartCheck != null)
            {
                DowBotLogger.Debug("Cancel is not null. You cannot create new Start!");
                return;
            }

            if (_socketClient.ConnectionState == ConnectionState.Connected || _socketClient.ConnectionState == ConnectionState.Connecting)
            {
                await Stop();
            }

            _isNormalStop = false;
            
            _cancelStartCheck = new CancellationTokenSource();
            Task.Delay(UpdatePeriod, _cancelStartCheck.Token).ContinueWith(x => StartCheck(), 
                _cancelStartCheck.Token, TaskContinuationOptions.NotOnCanceled, TaskScheduler.Default);
            DowBotLogger.Debug("DowBot launched connection loop!");
            await _socketClient.LoginAsync(TokenType.Bot, _botParams.GeneralModuleParams.Token);
            await _socketClient.StartAsync();
            DowBotLogger.Info("DowBot is started!");
        }

        private async Task StartCheck()
        {
            if (_cancelStartCheck != null)
            {
                _cancelStartCheck.Cancel();
                _cancelStartCheck.Dispose();
                _cancelStartCheck = null;
            }

            if (_isNormalStop || _socketClient.ConnectionState == ConnectionState.Connected)
                return;

            DowBotLogger.Debug("Bot could not connect to server. Trying again...");
            await Recreate();
        }

        /// <summary>
        /// Disconnect bot from Discord server
        /// </summary>
        /// <returns></returns>
        public async Task Stop()
        {
            if (_cancelStartCheck != null)
            {
                _cancelStartCheck.Cancel();
                _cancelStartCheck.Dispose();
                _cancelStartCheck = null;
            }
            _isNormalStop = true;
            await _socketClient.LogoutAsync();
            await _socketClient.StopAsync();
            DowBotLogger.Info("DowBot is stopped!");
        }

        /// <summary>
        /// Fully destroys bot. Use on program exit only.
        /// </summary>
        /// <returns></returns>
        public async Task Destroy()
        {
            await Stop();
            Deinitialize();
            DowBotLogger.Debug("DowBot is destroyed!");
        }
        
        /// <summary>
        /// Force stop and destroys the bot and creates it again. Workaround for unworking messages receiving.
        /// </summary>
        /// <returns></returns>
        public async Task Recreate()
        {
            await Destroy();
            Initialize();
            await Start();
            DowBotLogger.Debug("DowBot is restarted!");
        }
        
        #endregion

        private void Initialize()
        {
            _socketClient = new DiscordSocketClient();
            //_socketClient.Connected += BotOnConnected;
            _socketClient.Ready += BotOnReady;
            _socketClient.MessageReceived += BotOnMessageReceived;
            _socketClient.Disconnected += BotOnDisconnected;
            _socketClient.Log += BotOnLog;
            _socketClient.UserJoined += UserJoinedAsync;
            BotDatabase.InitDb();

            DowBotLogger.Debug("DowBot is initialized!");
        }

        private async Task UserJoinedAsync(SocketGuildUser arg)
        {
            var profile = BotDatabase.GetProfile(arg.Id);
            if (profile != null)
            {
                var userToMute = new List<SocketUser> { arg };
                if (profile.IsMuteActive)
                {
                    await AdminCommandsManager.MuteAsync(userToMute, profile.MuteUntil);
                }
            }
        }

        private void Deinitialize()
        {
            //_socketClient.Connected -= BotOnConnected;
            _socketClient.Ready -= BotOnReady;
            _socketClient.MessageReceived -= BotOnMessageReceived;
            _socketClient.Log -= BotOnLog;
            _socketClient.UserJoined -= UserJoinedAsync;
            _socketClient.Dispose();
            MainGuild = null;
            SyncChannel = null;
            _socketClient = null;
            OnBotReady = null;
            BotDatabase.DeInitDb();
            
            DowBotLogger.Debug("DowBot is DeInitialized successfully!");
        }
        

        private async Task BotOnMessageReceived(SocketMessage arg)
        {
            try
            {
                if (arg.Author.Id == _socketClient.CurrentUser.Id)
                    return;

                if (!arg.Content.StartsWith("!"))
                {
                    if (_botParams.SyncModuleParams == null || arg.Channel.Id != _botParams.SyncModuleParams.ChannelId)
                        return;


                    var nickName = (arg.Author as SocketGuildUser)?.Nickname ?? arg.Author.Username;
                    var text = arg.Content;
                    OnSyncMessageReceived?.Invoke(this, new SyncEventArgs(nickName, text));
                    return;
                }
                
                switch (arg.Channel)
                {
                    case SocketDMChannel _:
                        await _dmCommands.HandleBotCommand(arg);
                        break;
                    case SocketTextChannel guildChannel when guildChannel.Guild.Id == _botParams.GeneralModuleParams.MainGuildId:
                        await _guildCommands.HandleBotCommand(arg);
                        break;
                    default:
                        return;
                }
            }
            catch (Exception e)
            {
                DowBotLogger.Error("[DowBot.cs - BotOnMessageReceived] An unhandled error happened!.\n" + e);
            }
        }

        private async Task BotOnReady()
        {
            DowBotLogger.Trace("DowBot is ready!");
            MainGuild = _socketClient.GetGuild(_botParams.GeneralModuleParams.MainGuildId);
            if (_botParams.SyncModuleParams != null)
                SyncChannel = MainGuild.GetTextChannel(_botParams.SyncModuleParams.ChannelId);
            OnBotReady?.Invoke(this, MainGuild);
            _cancelUpdateLoop?.Dispose();
            _cancelUpdateLoop = new CancellationTokenSource();
            await Task.Run(UpdateLoop, _cancelUpdateLoop.Token);
        }

        private Task BotOnLog(LogMessage arg)
        {
            DowBotLogger.Debug(arg);
            return Task.CompletedTask;
        }
        
        private async Task ProcessAdminDynamic()
        {
            var timeNow = DateTime.UtcNow.Ticks;
            var unmuteList = new List<SocketUser>();
            foreach (var tableUser in BotDatabase.ProfilesTable.FindAll())
            {
                if (tableUser.IsMuteActive && tableUser.MuteUntil != -1 && timeNow >= tableUser.MuteUntil)
                {
                    unmuteList.Add(MainGuild.GetUser(tableUser.DiscordUserId));
                }
            }
            if (unmuteList.Count != 0)
            {
                await AdminCommandsManager.UnMuteAsync(unmuteList);
            }
        }

        private async Task ProcessDynamic()
        {
            foreach (var dynamicProvider in _botParams.DynamicModuleParams.DataProviders)
            {
                var text = dynamicProvider.Text;
                if (text == null)
                    continue;
                var channel = MainGuild.GetTextChannel(dynamicProvider.ChannelId);
                var messages = await channel.GetMessagesAsync(1).FlattenAsync();
                var message = messages.FirstOrDefault(x => x.Author.Id == _socketClient.CurrentUser.Id);
                if (message != null && message is RestUserMessage socketMessage)
                {
                    await socketMessage.ModifyAsync(x => x.Content = text);
                }
                else
                {
                    await channel.SendMessageAsync(dynamicProvider.Text);
                }
            }
        }
        
        private async Task UpdateLoop()
        {
            while (true)
            {
                try
                {
                    if (_cancelUpdateLoop.IsCancellationRequested)
                    {
                        _cancelUpdateLoop?.Dispose();
                        return;
                    }
                    
                    if (_botParams.AdministrativeModuleParams != null)
                    {
                        await ProcessAdminDynamic();
                    }

                    if (_botParams.DynamicModuleParams != null)
                    {
                        await ProcessDynamic();
                    }

                }
                catch (Exception ex)
                {
                    DowBotLogger.Warn(ex);
                }
                finally
                {
                    await Task.Delay(UpdatePeriod);
                }
            }
        }
        

        
        private async Task BotOnDisconnected(Exception arg)
        {
            _cancelUpdateLoop?.Cancel();
            if (_isNormalStop)
            {
                _isNormalStop = false;
                return;
            }
            DowBotLogger.Warn("Bot has been disconnected, enabled recreate task and it will be restored in 2 minutes!");
            await Task.Delay(UpdatePeriod * 2);
            await Recreate();
        }
    }
}