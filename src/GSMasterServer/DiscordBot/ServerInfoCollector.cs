using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using GSMasterServer.Servers;
using IrcNet.Tools;

namespace GSMasterServer.DiscordBot
{
    public class ServerInfoCollector
    {
        private readonly SingleMasterServer _singleMasterServer;
        private readonly BotManager _botManager;

        private static readonly string[] Numbers = new[] { ":one:", ":two:", ":three:", ":four:", ":five:", ":six:", ":seven:", ":eight:", ":nine:", ":one::zero:" };


        public ServerInfoCollector(SingleMasterServer singleMasterServer, BotManager botManager)
        {
            _singleMasterServer = singleMasterServer;
            _botManager = botManager;
        }


        public async Task UpdateServerMessage()
        {
            var textSb = new StringBuilder();
            var online = GetOnline();
            textSb.AppendLine($"➡️ Server Online: {online}\n");

            var top = _singleMasterServer.GetTop.Take(10).ToArray();
            textSb.AppendLine($"➡️ Best Players: ");
            for (var i = 0; i < top.Length; i++)
            {
                var p = top[i];
                textSb.AppendLine($"{Numbers[i]} **{p.Name}**  Rating: **{p.Score1v1}**  Games: **{p.GamesCount} ({Math.Round(p.WinRate * 100, 2)}%)**  Best Race: **{p.FavouriteRace}**");
            }

            var channel = _botManager.ThunderGuild.GetTextChannel(DiscordServerConstants.ThunderHawkInfoChannelId);
            var messages = await channel.GetMessagesAsync(1).FlattenAsync();
            var message = messages.FirstOrDefault(x => x.Author.Id == _botManager.BotClient.CurrentUser.Id);
            if (message != null && message is RestUserMessage socketMessage)
            {
                await socketMessage.ModifyAsync(x => x.Content = textSb.ToString());
            }
            else
            {
                await channel.SendMessageAsync(textSb.ToString());
            }
            Logger.Trace("[Discord bot]Updated server message!");


        }
        public int GetOnline()
        {
            return _singleMasterServer.Online;
        }

    }
}
