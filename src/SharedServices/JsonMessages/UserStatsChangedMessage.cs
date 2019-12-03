namespace SharedServices
{
    public class UserStatsChangedMessage : Message
    {
        public UserStatsChangedMessage()
            : base(MessageTypes.UserStatsChanged)
        {
        }

        public ulong SteamId { get; set; }
        public long ProfileId { get; set; }
        public string Name { get; set; }

        public long Games { get; set; }
        public long Wins { get; set; }
        public long Winstreak { get; set; }
        public long AverageDuration { get; set; }
        public long Disconnects { get; set; }
        public Race Race { get; set; }

        public long Delta { get; set; }
        public long CurrentScore { get; set; }
        public GameType GameType { get; set; }
    }
}
