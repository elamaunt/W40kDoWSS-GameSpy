namespace SharedServices
{
    public class LobbiesMessage : Message
    {
        public LobbiesMessage()
            : base(MessageTypes.Lobbies)
        {
        }

        public LobbyPart[] Lobbies { get; set; }
    }
}
