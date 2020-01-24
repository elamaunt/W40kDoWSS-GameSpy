using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using GSMasterServer.Servers;
using IrcNet.Tools;
using SharedServices;

namespace GSMasterServer.DiscordBot
{
    public class ServerInfoCollector
    {
        private readonly SingleMasterServer _singleMasterServer;
        private readonly BotManager _botManager;

        private static readonly string[] Numbers = { ":one:", ":two:", ":three:", ":four:", ":five:", ":six:", ":seven:", ":eight:", ":nine:", ":one::zero:" };

        private static string GetRaceName(Race race)
        {
            switch (race)
            {
                case Race.space_marine_race: return "Space Marines";
                case Race.chaos_marine_race: return "Chaos Space Marines";
                case Race.ork_race: return "Orks";
                case Race.eldar_race: return "Eldar";
                case Race.guard_race: return "Imperial Guard";
                case Race.necron_race: return "Necrons";
                case Race.tau_race: return "Tau";
                case Race.dark_eldar_race: return "Dark Eldar";
                case Race.sisters_race: return "Sisters of Battle";
                default: return "Unknown";
            }
        }


        public ServerInfoCollector(SingleMasterServer singleMasterServer, BotManager botManager)
        {
            _singleMasterServer = singleMasterServer;
            _botManager = botManager;
        }


        public void SendSyncMessage(string nickName, string text)
        {
            _singleMasterServer.HandleDiscordMessage(nickName, text);
        }


        public string GetPlayerInfo(string nickName, bool isAdmin)
        {
            var player = _singleMasterServer.GetPlayer(nickName);
            if (player == null)
                return "This player is not registered!";
            var sb = new StringBuilder();
            sb.AppendLine($"Nickname: **{player.Name}**");
            sb.AppendLine($"MMR 1v1: **{player.Score1v1}**, 2v2: **{player.Score2v2}**, 3v3: **{player.Score3v3}**");
            sb.AppendLine($"Wins: **{player.WinsCount}**, Games: **{player.GamesCount}** **({Math.Round(player.WinRate * 100, 2)}%)**, Winstreak: **{player.Best1v1Winstreak}**");
            sb.AppendLine(
                $"Games count: Sm: **{player.Smgamescount}**, Csm: **{player.Csmgamescount}**, Orks: **{player.Orkgamescount}**, Eldar: **{player.Eldargamescount}**, " +
                $"Ig: **{player.Iggamescount}**, Tau: **{player.Taugamescount}**, Necrons: **{player.Necrgamescount}**, Sob: **{player.Sobgamescount}**, De: **{player.Degamescount}**");
            //sb.AppendLine($"Favourite race: **{GetRaceName(player.FavouriteRace)}**");
            sb.AppendLine($"Time spent in battles: **{Math.Round(player.AllInGameTicks / 60f / 60f, 1)}** hours");
            sb.AppendLine($"Dowstats profile: https://dowstats.ru/player.php?sid={player.Id}&server=elSpy");
            if (isAdmin)
            {
                sb.AppendLine("--------------------------");
                sb.AppendLine($"Email: __**{player.Email}**__");
                sb.AppendLine($"Steam profile: __https://steamcommunity.com/profiles/{player.SteamId}__");
            }
            return sb.ToString();
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
                textSb.AppendLine($"{Numbers[i]} **{p.Name}**  Rating: **{p.Score1v1}**  Games: **{p.GamesCount} ({Math.Round(p.WinRate * 100, 2)}%)**  Best Race: **{GetRaceName(p.FavouriteRace)}**");
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
            // Logger.Trace("[Discord bot]Updated server message!"); // слишком много логов спамит
        }
        public int GetOnline()
        {
            return _singleMasterServer.Online;
        }

    }
}
