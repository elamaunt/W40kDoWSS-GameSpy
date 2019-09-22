namespace GSMasterServer
{
    public static class ServerConstants
    {
        public const string ModName = "ThunderHawk";
        public const string ModVersion = "1.0a";

        public static string ActiveGameVariant => (ModVersion + ModName).ToLowerInvariant();
    }
}
