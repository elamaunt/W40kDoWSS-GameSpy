using Framework;
using System.Collections.Generic;

namespace ThunderHawk
{
    public class LobbyMemberData
    {
        public ulong SteamId { get; set; }
        public string ShortUser { get; set; }
        public string Name { get; set; }

        Dictionary<string, string> KeyValues { get; set; }

        public string GetKeyValue(string key)
        {
            return KeyValues?.GetValueOrDefault(key);
        }
    }
}
