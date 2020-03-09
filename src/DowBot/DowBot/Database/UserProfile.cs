namespace DiscordBot.Database
{
    internal class UserProfile
    {
        /// <summary>
        /// Auto-incremented id
        /// </summary>
        public int Id { get; set; }
        
        public ulong DiscordUserId { get; set; }
        
        public long MuteUntil { get; set; }
        
        public bool IsMuteActive { get; set; }
        
        public bool IsRussianLanguage { get; set; }
    }
}