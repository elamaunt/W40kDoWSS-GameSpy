namespace GSMasterServer.DiscordBot.Database
{
    public class DiscordProfile
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public long MuteUntil { get; set; }
        public bool IsMuteActive { get; set; }
        public long SoftMuteUntil { get; set; }
        public bool IsSoftMuteActive { get; set; }
        public int Reputation { get; set; }
    }
}
