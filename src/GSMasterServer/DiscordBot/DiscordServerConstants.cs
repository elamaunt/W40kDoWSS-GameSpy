namespace GSMasterServer.DiscordBot
{
    public static class DiscordServerConstants
    {
#if Release
        public const ulong ServerId = 606832876369215491;

        public const ulong ThunderHawkInfoChannelId = 656215384453677056;
        
        public const ulong BotCategoryId = 608338832194273299;
        public const ulong ReadOnlyRoleId = 623924991599181824;  
        public const ulong FloodOnlyRoleId = 623925234030084100;

        public const ulong AdminRoleId = 623907787788910613;
        public const ulong ModerRoleId = 623909305669910549;

#endif

#if Debug || SPACEWAR
        public const ulong ServerId = 624305167743057921;

        public const ulong ThunderHawkInfoChannelId = 635516606465835030;

        public const ulong BotCategoryId = 624962245914525698;
        public const ulong ReadOnlyRoleId = 624963432105639936;
        public const ulong FloodOnlyRoleId = 624963455002083329;

        public const ulong AdminRoleId = 624310819639001094;
        public const ulong ModerRoleId = 624310824550400010;
#endif
    }
}
