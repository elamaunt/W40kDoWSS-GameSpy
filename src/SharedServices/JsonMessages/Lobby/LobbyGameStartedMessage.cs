namespace SharedServices
{
    public class LobbyGameStartedMessage : Message
    {
        public LobbyGameStartedMessage() 
            : base(MessageTypes.LobbyGameStarted)
        {
        }
    }
}
