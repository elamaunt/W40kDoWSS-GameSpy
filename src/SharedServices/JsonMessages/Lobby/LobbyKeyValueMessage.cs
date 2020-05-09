namespace SharedServices
{
    public class LobbyKeyValueMessage : Message
    {
        public LobbyKeyValueMessage()
            : base(MessageTypes.LobbyKeyValue)
        {
        }

        public ulong? HostSteamId { get; set; }
        public ulong? SteamId { get; set; }
        public string Name { get; set; }
        public long? ProfileId { get; set; }

        public string Key { get; set; }
        public string Value { get; set; }
    }
}
