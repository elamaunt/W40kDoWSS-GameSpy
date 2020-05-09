namespace SharedServices
{
    public class LobbyChatLineMessage : Message
    {
        public ulong? HostSteamId { get; set; }
        public ulong? SteamId { get; set; }

        public string Nick { get; set; }
        public string Line { get; set; }
        public LobbyChatLineMessage()
            : base(MessageTypes.LobbyChatLine)
        {
        }
    }
}
