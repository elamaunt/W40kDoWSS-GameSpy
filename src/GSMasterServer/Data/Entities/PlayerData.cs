using SharedServices;

namespace GSMasterServer
{
    public class PlayerData
    {
        public long ProfileId { get; set; }
        public long Rating { get; set; }
        public long RatingDelta { get; set; }
        public byte Team { get; set; }
        public PlayerFinalState FinalState { get; set; }
        public Race Race { get; set; }
    }
}
