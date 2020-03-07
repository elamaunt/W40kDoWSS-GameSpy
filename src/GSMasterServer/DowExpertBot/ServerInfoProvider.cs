using System;
using System.Linq;
using System.Text;
using DiscordBot.Commands.DynamicModule;
using GSMasterServer.Servers;
using SharedServices;

namespace GSMasterServer.DowExpertBot
{
    public class ServerInfoProvider: IDynamicDataProvider
    {
        public string Text
        {
            get
            {
                var textSb = new StringBuilder();
                var online = GetOnline();
                textSb.AppendLine($"➡️ Server Online: {online}\n");

                var top = _singleMasterServer.GetTop.Take(10).ToArray();
                textSb.AppendLine($"➡️ Best Players: ");
                for (var i = 0; i < top.Length; i++)
                {
                    var p = top[i];
                    textSb.AppendLine(
                        $"{Numbers[i]} **{p.Name}**  Rating: **{p.Score1v1}**  Games: **{p.GamesCount} ({Math.Round(p.WinRate * 100, 2)}%)**  Best Race: **{GetRaceName(p.FavouriteRace)}**");
                }

                return textSb.ToString();
            }
        }
        
        public ulong ChannelId { get; } = DiscordServerConstants.ThunderHawkInfoChannelId;

        private readonly SingleMasterServer _singleMasterServer;
        public ServerInfoProvider(SingleMasterServer singleMasterServer)
        {
            _singleMasterServer = singleMasterServer;
        }
        
        private static readonly string[] Numbers =
            {":one:", ":two:", ":three:", ":four:", ":five:", ":six:", ":seven:", ":eight:", ":nine:", ":one::zero:"};
        
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
        
        public int GetOnline()
        {
            return _singleMasterServer.Online;
        }
    }
}