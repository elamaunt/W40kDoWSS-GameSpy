using System.Collections.Generic;

namespace ThunderHawk
{
    public class LobbyData
    {
        public int MaxMembers { get; set; }
        public bool Joinable { get; set; }
        public bool IsRated { get; set; }
        public string Indicator { get; set; }
        public LobbyMemberData Host { get; set; }
        public LobbyMemberData LocalPlayer { get; set; }
        public List<LobbyMemberData> Members { get; set; }
    }
}
