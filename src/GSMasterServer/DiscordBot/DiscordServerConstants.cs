namespace GSMasterServer.DiscordBot
{
    public static class DiscordServerConstants
    {
#if Release
        public const long serverId = 606832876369215491;
        public const long welcomeChannelId = 606832876369215495;
        public const long adminRoleId = 623907787788910613;
        public const long moderRoleId = 623909305669910549;
#endif

#if Debug
        public const long adminRoleId = 624310819639001094;
        public const long moderRoleId = 624310824550400010;
#endif
    }
}
