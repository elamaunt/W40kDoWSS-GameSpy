namespace SharedServices
{
    public class LobbyLeftMessage : Message
    {
        public LobbyLeftMessage()
            : base(MessageTypes.LobbyLeft)
        {
        }

        public ulong HostSteamId { get; set; }
        public ulong SteamId { get; set; }
        public string Nick { get; set; }
        public long ProfileId { get; set; }
    }
}
