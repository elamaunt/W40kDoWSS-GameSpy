using SharedServices;

namespace ThunderHawk.Core
{
    public class GameInfo
    {
        public string Map { get; set; }
        public string ModName { get; set; }
        public string ModVersion { get; set; }
        public bool IsRateGame { get; set; }
        public GameType Type { get; set; }
        public PlayerInfo[] Players { get; set; }
        public string SessionId { get; set; }
    }
}
