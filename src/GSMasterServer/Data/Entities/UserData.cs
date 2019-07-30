namespace GSMasterServer.Data
{
    public class UserData
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long ProfileId { get; set; }
        public string Name { get; set; }
        public string Passwordenc { get; set; }
        public string Email { get; set; }
        public string Country { get; set; }
        public ulong SteamId { get; set; }
        public long Session { get; set; }
        public string LastIp { get; set; }
    }
}