namespace ThunderHawk.Utils
{
    internal class GameConstants
    {
        public const string VERSION = "1.2 BETA";

#if SPACEWAR
        public const string SERVER_ADDRESS = "192.168.159.1";
#else
        public const string SERVER_ADDRESS = "139.59.210.74";
#endif
        //public const string SERVER_ADDRESS = "127.0.0.1";
    }
}