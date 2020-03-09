using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DiscordBot;
using DiscordBot.BotParams;
using DiscordBot.Commands;
using DiscordBot.Commands.DynamicModule;
using DiscordBot.Commands.Primitives;
using DiscordBot.Commands.SyncModule;
using GSMasterServer.Servers;

namespace GSMasterServer.DowExpertBot
{
    public class BotManager
    {
        private readonly SingleMasterServer _singleMasterServer;
        private readonly DowBot _dowBot;
        public BotManager(SingleMasterServer singleMasterServer)
        {
            var token = File.ReadAllText("discord_token.txt");
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Token is empty! Bot stopped working...");

            var botParams = new BotParams(new List<IModuleParams>
            {
                new GeneralModuleParams(token, DiscordServerConstants.ThunderGuildId, new DowLogger()),
                new RandomModuleParams( new[]{ DiscordServerConstants.BotChannelId }),
                new AdministrativeModuleParams(DiscordServerConstants.AdminRoleId, DiscordServerConstants.ModerRoleId, DiscordServerConstants.ReadOnlyRoleId),
                new DynamicModuleParams(new List<IDynamicDataProvider> {new ServerInfoProvider(singleMasterServer)}),
                new SyncModuleParams(DiscordServerConstants.SyncChatId),
                new CustomCommandsModuleParams(null, new Dictionary<string, DmCommand> { {"getp", new GetPlayerCommand(singleMasterServer, new 
                DmCommandParams(CommandAccessLevel.Everyone))} })
            });
            _dowBot = new DowBot(botParams);
            //_dowBot.OnSyncMessageReceived += DowBotOnOnSyncMessageReceived;

            _singleMasterServer = singleMasterServer;
            //_singleMasterServer.OnChatMessageReceived += OnChatMessageReceived;
        }
        
        public async Task LaunchBot()
        {
            await _dowBot.Start();
        }


        public void DestroyBot()
        {
            _dowBot?.Destroy();
        }

        private void OnChatMessageReceived(object sender, SharedServices.ChatMessageMessage e)
        {
            _dowBot?.SendSyncMessage(e.UserName, e.Text);
        }


        private void DowBotOnOnSyncMessageReceived(object sender, SyncEventArgs e)
        {
            _singleMasterServer.HandleDiscordMessage(e.Author, e.Text);
        }
    }
}