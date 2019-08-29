using GSMasterServer.Servers;
using System.Net;

namespace ThunderHawk
{
    internal static class ServerContext
    {
        public static ServerListReport ServerListReport { get; private set; }
        public static ServerListRetrieve ServerListRetrieve { get; private set; }
        public static LoginServerRetranslator LoginMasterServer { get; private set; }
        public static ChatServerRetranslator ChatServer { get; private set; }

        public static void Start(IPAddress bind)
        {
            ServerListReport = new ServerListReport(bind, 27900);
            ServerListRetrieve = new ServerListRetrieve(bind, 28910);
            LoginMasterServer = new LoginServerRetranslator(bind, 29900, 29901);
            ChatServer = new ChatServerRetranslator(bind, 6667);
        }
    }
}