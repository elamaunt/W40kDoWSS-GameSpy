namespace GSMasterServer
{
    public static class ServerConstants
    {
        public const string ModName = "ThunderHawk";
        public const string ModVersion = "1.1";

        public static string ActiveGameVariant => (ModVersion + ModName).ToLowerInvariant();
    }
}
