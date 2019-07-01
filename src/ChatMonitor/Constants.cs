namespace ChatMonitor
{
    public static class Constants
    {
        public const string NETWORK = "peerchat.gamespy.com";
        public const int PORT = 6667;
        public const string NICK = "ChatMonitor-gs";
        public const string ALT_NICK = "ChatMonitor-gs";
        public static readonly string[] CHAN = new string[]
        {
            "#gsp!subhome",
            "#gsp!gsarcadetour",
            "#gsp!chatmain",
            "#gsp!servers",
            "#gsp!arena",
            "#gsp!livewire",
            "#GSP!webgames",
            "#gsp!fileplanet"
        };

        public const string IDENTD = "XaaaaaaaaX|10008";
        public const string REALNAME = "ChatMonitor";
        public static bool CONNECTED = false;
        public const string OPER_NAME = "ChatMonitor";
        public const string OPER_EMAIL = "chatmonitor@gamespy.com";
        public const string OPER_PASSWORD = "ihatethiswork";
    }
}
