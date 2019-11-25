namespace SharedServices
{
    public class RequestUserStatsMessage : Message
    {
        public RequestUserStatsMessage()
            : base(MessageTypes.RequestUserStats)
        {
        }

        public string Name { get; set; }
        public long? ProfileId { get; set; }
    }
}
