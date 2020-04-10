using System.Collections.ObjectModel;

namespace SharedServices
{
    public class LobbyPart
    {
        public ulong HostSteamId { get; set; }
        public string Indicator { get; set; }
        public ReadOnlyDictionary<string, string> Properties { get; set; }
        public int MaxPlayers { get; set; }
        public int PlayersCount { get; set; }
    }
}
