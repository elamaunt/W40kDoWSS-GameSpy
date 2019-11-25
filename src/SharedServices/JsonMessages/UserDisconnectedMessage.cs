namespace SharedServices
{
    public class UserDisconnectedMessage : Message
    {
        public UserDisconnectedMessage()
            : base(MessageTypes.UserDisconnected)
        {
        }

        public ulong SteamId { get; set; }
    }
}
