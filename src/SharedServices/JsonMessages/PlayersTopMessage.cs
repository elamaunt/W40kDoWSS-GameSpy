namespace SharedServices
{
    public class PlayersTopMessage
    {
        public UserStatsMessage[] Stats { get; set; }
        public int Offset { get; set; }
        public int Count { get; set; }
    }
}
