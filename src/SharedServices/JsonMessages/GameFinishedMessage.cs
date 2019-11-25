namespace SharedServices
{
    public class GameFinishedMessage : Message
    {
        public GameFinishedMessage()
            : base(MessageTypes.GameFinished)
        {
        }

        public string SessionId { get; set; }
        public string ModName { get; set; }
        public string ModVersion { get; set; }
        public GameType Type { get; set; }
        public long Duration { get; set; }
        public PlayerPart[] Players { get; set; }
        public bool IsRateGame { get; set; }
    }
}
