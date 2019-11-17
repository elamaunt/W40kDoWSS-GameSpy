using System;
using SharedServices;

namespace GSMasterServer
{
    public class GameDBO
    {
        public string Id { get; set; }
        public string ModName { get; set; }
        public string ModVersion { get; set; }
        public string Uri { get; set; }
        public bool IsRateGame { get; set; }
        public ulong UploadedBy { get; set; }
        public DateTime UploadedDate { get; set; }
        public long Duration { get; set; }
        public PlayerData[] Players { get; set; }
        public GameType Type { get; set; }
    }
}
