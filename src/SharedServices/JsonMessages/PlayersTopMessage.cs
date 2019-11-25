namespace SharedServices
{
    public class PlayersTopMessage : Message
    {
        public PlayersTopMessage()
            : base(MessageTypes.PlayersTop)
        {
        }

        public UserStatsMessage[] Stats { get; set; }
        public int Offset { get; set; }
        public int Count { get; set; }
    }
}
