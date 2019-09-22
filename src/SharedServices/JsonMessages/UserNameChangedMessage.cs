namespace SharedServices
{
    public class UserNameChangedMessage
    {
        public ulong SteamId { get; set; }
        public string NewName { get; set; }
    }
}
