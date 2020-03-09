namespace GSMasterServer.DowExpertBot
{
    public static class DiscordServerConstants
    {
#if Release
        public const ulong ThunderGuildId = 606832876369215491;

        public const ulong ThunderHawkInfoChannelId = 656215384453677056;

        public const ulong BotChannelId = 669510712531615754;

        public const ulong ReadOnlyRoleId = 623924991599181824;  
        public const ulong SyncChatId = 659007052370411550;


        public const ulong AdminRoleId = 623907787788910613;
        public const ulong ModerRoleId = 623909305669910549;

#endif

#if Debug || SPACEWAR
        public const ulong ThunderGuildId = 624305167743057921;

        public const ulong ThunderHawkInfoChannelId = 685892199740735545;

        public const ulong BotChannelId = 669510610869944331;

        public const ulong ReadOnlyRoleId = 624963432105639936;
        public const ulong SyncChatId = 635516606465835030;

        public const ulong AdminRoleId = 624310819639001094;
        public const ulong ModerRoleId = 624310824550400010;
#endif
    }
}
