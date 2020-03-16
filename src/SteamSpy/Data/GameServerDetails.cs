using Steamworks;
using System;
using System.Collections.Generic;

namespace ThunderHawk
{
    public class GameServerDetails
    {
        readonly Dictionary<string, string> _values = new Dictionary<string, string>();

        public Dictionary<string, string> Properties => _values;

        public void Set(string key, string value)
        {
            _values[key] = value;
        }

        public string this[string key]
        {
            set
            {
                _values[key] = value;
            }
            get
            {
                return GetOrDefault(key);
            }
        }
        public string RoomHash { get; set; }
        public CSteamID HostSteamId { get; set; }
        public CSteamID LobbySteamId { get; set; }

        public bool Ranked => GameName == "whamdowfram";
        public bool IsTeamplay => GetOrDefault("teamplay") == "1";
        public bool HasPlayers => Int32.Parse(GetOrDefault("numplayers")) > 0;
        public string PlayersCount => GetOrDefault("numplayers");
        public string HostPort => GetOrDefault("hostport");
        public string HostName => GetOrDefault("hostname");
        public string StateChanged => GetOrDefault("statechanged");
        public string MaxPlayers => GetOrDefault("maxplayers");
        public string GameVer => GetOrDefault("gamever");
        public string GameName => GetOrDefault("gamename") ?? GetOrDefault("GameName");
        public string GameType => GetOrDefault("gametype");
        public string GameVariant => GetOrDefault("gamevariant");
        public int Score => GetOrDefault("score_").ParseToIntOrDefault();

        public bool LobbyLimited
        {
            get => GetOrDefault("limitedByRating") == "1";
            set => this["limitedByRating"] = value ? "1" : "0";
        }

        public string GetOrDefault(string key)
        {
            _values.TryGetValue(key, out string value);
            return (string)value;
        }

        public bool IsValid => !String.IsNullOrWhiteSpace(HostName) &&
                !String.IsNullOrWhiteSpace(GameVariant) &&
                !String.IsNullOrWhiteSpace(GameVer) &&
                !String.IsNullOrWhiteSpace(GameType) &&
                MaxPlayers != "0";

    }
}
