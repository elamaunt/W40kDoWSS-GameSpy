namespace SharedServices
{
    public class UserStatsChangedMessage
    {
        public ulong SteamId { get; set; }
        public long ProfileId { get; set; }
        public string Name { get; set; }

        public int Delta { get; set; }
        public int CurrentScore { get; set; }
        public GameType GameType { get; set; }
    }
}
