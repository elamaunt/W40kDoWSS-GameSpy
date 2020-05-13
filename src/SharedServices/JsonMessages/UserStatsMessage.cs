namespace SharedServices
{
    public class UserStatsMessage : Message
    {
        public UserStatsMessage()
            : base(MessageTypes.UserStats)
        {
        }

        public ulong SteamId { get; set; }
        public long Modified { get; set; }
        public long ProfileId { get; set; }
        public string Name { get; set; }
        public Race FavouriteRace { get; set; }
        public long GamesCount { get; set; }
        public long WinsCount { get; set; }
        public long Score1v1 { get; set; }
        public long Score2v2 { get; set; }
        public long Score3v3_4v4 { get; set; }
        public long ScoreBattleRoyale { get; set; }
        public long AverageDuration { get; set; }
        public long Disconnects { get; set; }
        public long Best1v1Winstreak { get; set; }
    }
}
