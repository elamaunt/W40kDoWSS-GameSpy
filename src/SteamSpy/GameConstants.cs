namespace ThunderHawk.Utils
{
    internal class GameConstants
    {
        public const string VERSION = "1.0";

#if SPACEWAR
       // public const string SERVER_ADDRESS = "192.168.159.1";
        public const string SERVER_ADDRESS = "127.0.0.1";
#else
        public const string SERVER_ADDRESS = "134.209.227.145";
#endif

        //public const string SERVER_ADDRESS = "127.0.0.1";

        public const ushort IDS_REQUEST_PORT = 27902;

        //public const ushort CLIENT_LOGIN_TCP_PORT = 29900;
        //public const ushort SEARCH_LOGIN_TCP_PORT = 29901;

        public const ushort CLIENT_LOGIN_TCP_PORT = 29902;
        public const ushort SEARCH_LOGIN_TCP_PORT = 29903;
    }
}