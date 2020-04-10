namespace SharedServices
{
    public class LobbyCreatedMessage : Message
    {
        public LobbyCreatedMessage()
            : base(MessageTypes.LobbyCreated)
        {
        }
    }
}
