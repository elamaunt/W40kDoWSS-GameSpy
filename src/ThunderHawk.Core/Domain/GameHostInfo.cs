namespace ThunderHawk.Core
{
    public class GameHostInfo
    {
        public bool IsUser { get; set; }
        public bool Teamplay { get; set; }
        public bool Ranked { get; set; }
        public string GameVariant { get; set; }
        public int MaxPlayers { get; set; }
        public int Players { get; set; }
        public int Score { get; set; }
        public bool LimitedByRating { get; set; }
    }
}
