using System;
using System.Collections.Generic;
using System.IO;
using DiscordBot;
using DiscordBot.BotParams;
using DiscordBot.Commands.DynamicModule;
using DiscordBot.Commands.SyncModule;
using GSMasterServer.Servers;

namespace GSMasterServer.DowExpertBot
{
    public class BotManager: IDisposable
    {
        private readonly SingleMasterServer _singleMasterServer;

        private DowBot _dowBot;

        
        public BotManager(SingleMasterServer singleMasterServer)
        {
            _singleMasterServer = singleMasterServer;
            _singleMasterServer.OnChatMessageReceived += OnChatMessageReceived;
        }
        
        public void LaunchBot()
        {
            var token = File.ReadAllText("discord_token.txt");
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Token is empty! Bot stopped working...");
            var botParams = new BotParams(new List<IModuleParams>
            {
                new GeneralModuleParams(DiscordServerConstants.ServerId),
                new RandomModuleParams( new ulong[]{ DiscordServerConstants.BotChannelId }),
                new AdministrativeModuleParams(DiscordServerConstants.AdminRoleId, DiscordServerConstants.ModerRoleId, DiscordServerConstants.ReadOnlyRoleId),
                new DynamicModuleParams(new List<IDynamicDataProvider> {new ServerInfoProvider(_singleMasterServer)}),
                new SyncModuleParams(DiscordServerConstants.SyncChatId),
            });
            _dowBot = new DowBot(token, new DowLogger(), botParams);

            _dowBot.OnSyncMessageReceived += DowBotOnOnSyncMessageReceived;

            _dowBot.StartLoop();
        }

        private void OnChatMessageReceived(object sender, SharedServices.ChatMessageMessage e)
        {
            _dowBot?.SendSyncMessage(e.UserName, e.Text);
        }
        
        public void DestroyBot()
        {
            _dowBot?.Destroy();
        }

        private void DowBotOnOnSyncMessageReceived(object sender, SyncEventArgs e)
        {
            _singleMasterServer.HandleDiscordMessage(e.Author, e.Text);
        }

        public void Dispose()
        {
            DestroyBot();
        }
    }
}