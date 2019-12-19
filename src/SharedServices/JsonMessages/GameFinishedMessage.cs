using System;

namespace SharedServices
{
    public class GameFinishedMessage : Message
    {
        public GameFinishedMessage()
            : base(MessageTypes.GameFinished)
        {
        }

        public string Url { get; set; }
        public string Map { get; set; }
        public string SessionId { get; set; }
        public string ModName { get; set; }
        public string ModVersion { get; set; }
        public GameType Type { get; set; }
        public long Duration { get; set; }
        public DateTime Date { get; set; }
        
        public PlayerPart[] Players { get; set; }
        public bool IsRateGame { get; set; }
    }
}
