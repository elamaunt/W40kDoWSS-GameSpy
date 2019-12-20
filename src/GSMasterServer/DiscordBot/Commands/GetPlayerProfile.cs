using System.Threading.Tasks;
using Discord.WebSocket;

namespace GSMasterServer.DiscordBot.Commands
{
    internal class GetPlayerProfile : IBotDmCommand
    {
        public AccessLevel MinAccessLevel { get; } = AccessLevel.User;
        public async Task Execute(SocketMessage socketMessage, BotManager botManager, AccessLevel accessLevel)
        {
            var args = socketMessage.CommandArgs();
            if (args.Length <= 0)
                return;
            var player = args[0];

            var info = botManager.ServerInfoCollector.GetPlayerInfo(player, accessLevel == AccessLevel.Admin);
            await socketMessage.Channel.SendMessageAsync(info);
        }
    }
}
