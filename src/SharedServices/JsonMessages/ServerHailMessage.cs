namespace SharedServices
{
    public class ServerHailMessage
    {
        public ulong SteamId { get; set; }
        public string ModName { get; set; }
        public string ModVersion { get; set; }
        public string ActiveGameVariant { get; set; }
    }
}
