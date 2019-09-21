namespace GSMasterServer.DiscordBot.Database
{
    public class DiscordProfile
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public ulong MuteUntil { get; set; }
        public ulong SoftMuteUntil { get; set; }
    }
}
