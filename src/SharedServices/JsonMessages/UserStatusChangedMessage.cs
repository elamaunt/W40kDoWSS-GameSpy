namespace SharedServices
{
    public class UserStatusChangedMessage : Message
    {
        public UserStatusChangedMessage()
            : base(MessageTypes.UserStatusChanged)
        {
        }

        public ulong SteamId { get; set; }
        public string Status { get; set; }
    }
}
