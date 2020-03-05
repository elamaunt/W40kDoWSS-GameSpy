namespace DiscordBot.Database
{
    internal class UserProfile
    {
        public uint Id { get; set; }
        
        public ulong DiscordUserId { get; set; }
        
        public long MuteUntil { get; set; }
        
        public bool IsMuteActive { get; set; }
    }
}