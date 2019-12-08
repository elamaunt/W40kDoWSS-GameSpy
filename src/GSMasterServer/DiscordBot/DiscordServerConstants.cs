namespace GSMasterServer.DiscordBot
{
    public static class DiscordServerConstants
    {
#if Release
        public const ulong serverId = 606832876369215491;
        public const ulong welcomeChannelId = 606832876369215495;

        public const ulong logChannelId = 626123363840163861;

        public const ulong normalCatRuId = 618843467237425153;
        public const ulong normalCatEnId = 622125594351501313;
        public const ulong floodCatId = 608338832194273299;

        public const ulong readOnlyRoleId = 623924991599181824;  
        public const ulong floodOnlyRoleId = 623925234030084100;

        public const ulong adminRoleId = 623907787788910613;
        public const ulong moderRoleId = 623909305669910549;
#else


        public const ulong serverId = 624305167743057921;

        public const ulong logChannelId = 626123183380234261;

        public const ulong normalCatRuId = 624962194051956739;
        public const ulong normalCatEnId = 624962194051956739;
        public const ulong floodCatId = 624962245914525698;

        public const ulong readOnlyRoleId = 624963432105639936;
        public const ulong floodOnlyRoleId = 624963455002083329;

        public const ulong adminRoleId = 624310819639001094;
        public const ulong moderRoleId = 624310824550400010;
#endif
    }
}
