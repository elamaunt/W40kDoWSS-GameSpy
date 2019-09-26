using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class SingleClientServer : IClientServer
    {
        UdpPortHandler _serverReport;
        TcpPortHandler _serverRetrieve;
        TcpPortHandler _clientManager;
        TcpPortHandler _searchManager;
        TcpPortHandler _chat;
        TcpPortHandler _stats;
        TcpPortHandler _http;


        bool _chatEncoded;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Start()
        {
            _serverReport = new UdpPortHandler(27900, OnServerReport);
            _serverRetrieve = new TcpPortHandler(28910, OnServerRetrieve);

            _clientManager = new TcpPortHandler(29900, OnClientManager);
            _searchManager = new TcpPortHandler(29901, OnSearchManager);

            _chat = new TcpPortHandler(6667, OnChat, OnChatAccept);
            _stats = new TcpPortHandler(29920, OnStats);
            _http = new TcpPortHandler(80, OnHttp);
            //ServerListReport = new ServerListReport(bind, 27900);
            //ServerListRetrieve = new ServerListRetrieve(bind, 28910);
            //LoginMasterServer = new LoginServerRetranslator(bind, 29900, 29901);
            //ChatServer = new ChatServerRetranslator(bind, 6667);
        }

        void OnChatAccept(TcpPortHandler handler, TcpClient client, CancellationToken token)
        {
            _chatEncoded = false;
        }

        void OnClientManager(TcpPortHandler handler, byte[] buffer, int count)
        {
            var str = ToUtf8(buffer, count);
        }

        void OnSearchManager(TcpPortHandler handler, byte[] buffer, int count)
        {
            var str = ToUtf8(buffer, count);
        }

        void OnHttp(TcpPortHandler handler, byte[] buffer, int count)
        {
            var str = ToUtf8(buffer, count);
        }

        void OnStats(TcpPortHandler handler, byte[] buffer, int count)
        {
            var str = ToUtf8(buffer, count);
        }

        void OnChat(TcpPortHandler handler, byte[] buffer, int count)
        {
            var str = ToUtf8(buffer, count);
        }

        void OnServerRetrieve(TcpPortHandler handler, byte[] buffer, int count)
        {
            var str = ToUtf8(buffer, count);
        }

        void OnServerReport(UdpPortHandler handler, UdpReceiveResult result)
        {
            var str = ToUtf8(result.Buffer, result.Buffer.Length);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Stop()
        {

        }

        string ToUtf8(byte[] buffer, int count)
        {
            return Encoding.UTF8.GetString(buffer, 0, count);
        }

        string ToASCII(byte[] buffer, int count)
        {
            return Encoding.ASCII.GetString(buffer, 0, count);
        }
    }
}
