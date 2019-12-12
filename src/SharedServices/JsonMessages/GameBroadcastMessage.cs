namespace SharedServices
{
    public class GameBroadcastMessage : Message
    {
        public GameBroadcastMessage()
            : base(MessageTypes.GameBroadcast)
        {
        }

        public ulong HostSteamId { get; set; }
        public bool Teamplay { get; set; }
        public bool Ranked { get; set; }
        public string GameVariant { get; set; }
        public int MaxPlayers { get; set; }
        public int Players { get; set; }
    }
}
