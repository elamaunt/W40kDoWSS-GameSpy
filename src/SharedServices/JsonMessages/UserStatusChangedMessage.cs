namespace SharedServices
{
    public class UserStatusChangedMessage
    {
        public ulong SteamId { get; set; }
        public string Status { get; set; }
    }
}
