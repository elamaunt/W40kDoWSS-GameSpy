namespace SharedServices
{
    public class UserConnectedMessage : Message
    {
        public UserConnectedMessage()
            : base(MessageTypes.UserConnected)
        {
        }

        public ulong SteamId { get; set; }
    }
}
