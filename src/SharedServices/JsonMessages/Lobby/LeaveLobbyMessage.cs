namespace SharedServices
{
    public class LeaveLobbyMessage : Message
    {
        public LeaveLobbyMessage()
            : base(MessageTypes.LeaveLobby)
        {
        }
    }
}
