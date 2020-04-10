namespace SharedServices
{
    public class EnteredInLobbyMessage : Message
    {
        public EnteredInLobbyMessage() 
            : base(MessageTypes.EnteredInLobby)
        {
        }

        public ulong SteamId { get; set; }
        public string Nick { get; set; }
        public long ProfileId { get; set; }
    }
}
