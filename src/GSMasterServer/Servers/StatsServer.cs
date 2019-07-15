using GSMasterServer.Data;
using GSMasterServer.Utils;
using IrcD.Core;
using IrcD.Core.Utils;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GSMasterServer.Servers
{
    internal class StatsServer : Server
    {
        const string Category = "Stats";

        const string XorKEY = "GameSpy3D";

        Thread _thread;
        Socket _newPeerAceptingsocket;
        readonly ManualResetEvent _reset = new ManualResetEvent(false);
        readonly IrcDaemon _ircDaemon;

        public StatsServer(IPAddress address, ushort port)
        {
            _thread = new Thread(StartServer)
            {
                Name = "Stats Socket Thread"
            };

            _thread.Start(new AddressInfo()
            {
                Address = address,
                Port = port
            });

            _ircDaemon = new IrcDaemon(IrcMode.Modern);
        }

        private void StartServer(object parameter)
        {
            AddressInfo info = (AddressInfo)parameter;

            Log(Category, "Init");

            try
            {
                _newPeerAceptingsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = 5000,
                    ReceiveTimeout = 5000,
                    SendBufferSize = 65535,
                    ReceiveBufferSize = 65535
                };

                _newPeerAceptingsocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
                _newPeerAceptingsocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

                _newPeerAceptingsocket.Bind(new IPEndPoint(info.Address, info.Port));
                _newPeerAceptingsocket.Listen(10);

            }
            catch (Exception e)
            {
                LogError(Category, String.Format("Unable to bind Server List Retrieval to {0}:{1}", info.Address, info.Port));
                LogError(Category, e.ToString());
                return;
            }

            while (true)
            {
                _reset.Reset();
                _newPeerAceptingsocket.BeginAccept(AcceptCallback, _newPeerAceptingsocket);
                _reset.WaitOne();
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            _reset.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket socket = listener.EndAccept(ar);

            SocketState state = new SocketState()
            {
                Socket = socket
            };
            
            socket.Send(XorBytes(@"\lc\1\challenge\KNDVKXFQWP\id\1\final\", XorKEY, 7));
            WaitForData(state);
        }

        private void WaitForData(SocketState state)
        {
            Thread.Sleep(10);
            if (state == null || state.Socket == null || !state.Socket.Connected)
                return;

            try
            {
                state.Socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, OnDataReceived, state);
            }
            catch (ObjectDisposedException)
            {
                state.Socket = null;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.NotConnected)
                    return;

                LogError(Category, "Error receiving data");
                LogError(Category, String.Format("{0} {1}", e.SocketErrorCode, e));
                return;
            }
        }



        private unsafe void OnDataReceived(IAsyncResult async)
        {
            SocketState state = (SocketState)async.AsyncState;

            if (state == null || state.Socket == null || !state.Socket.Connected)
                return;

            try
            {
                // receive data from the socket
                int received = state.Socket.EndReceive(async);

                if (received == 0)
                    return;

                var buffer = state.Buffer;

                var input = Encoding.UTF8.GetString(XorBytes(buffer, 0, received - 7, XorKEY), 0, received);

                Log(Category, input);

                if (input.StartsWith(@"\auth\\gamename\"))
                {
                    SendToClient(state, @"\lc\2\sesskey\1482017401\proof\0\id\1\final\");

                    goto CONTINUE;
                }

                if (input.StartsWith(@"\authp\\pid\"))
                {
                    //var clientData = LoginDatabase.Instance.GetData(state.Name);


                    // \authp\\pid\87654321\resp\67512e365ba89497d60963caa4ce23d4\lid\1\final\

                    // \authp\\pid\87654321\resp\7e2270c581e8daf5a5321ff218953035\lid\1\final\

                    var pid = input.Substring(12, 8);

                    SendToClient(state, $@"\pauthr\{pid}\lid\1\final\");
                    //SendToClient(state, @"\pauthr\-3\lid\1\errmsg\helloworld\final\");
                    //SendToClient(state, @"\pauthr\100000004\lid\1\final\");

                    goto CONTINUE;
                }

                
                if (input.StartsWith(@"\getpd\"))
                {
                    // \\getpd\\\\pid\\87654321\\ptype\\3\\dindex\\0\\keys\\\u0001points\u0001points2\u0001points3\u0001stars\u0001games\u0001wins\u0001disconn\u0001a_durat\u0001m_streak\u0001f_race\u0001SM_wins\u0001Chaos_wins\u0001Ork_wins\u0001Tau_wins\u0001SoB_wins\u0001DE_wins\u0001Eldar_wins\u0001IG_wins\u0001Necron_wins\u0001lsw\u0001rnkd_vics\u0001con_rnkd_vics\u0001team_vics\u0001mdls1\u0001mdls2\u0001rg\u0001pw\\lid\\1\\final\\
                    // \getpd\\pid\87654321\ptype\3\dindex\0\keys\pointspoints2points3starsgameswinsdisconna_duratm_streakf_raceSM_winsChaos_winsOrk_winsTau_winsSoB_winsDE_winsEldar_winsIG_winsNecron_winslswrnkd_vicscon_rnkd_vicsteam_vicsmdls1mdls2rgpw\lid\1\final\

                    var pid = input.Substring(12, 8);

                    var keysIndex = input.IndexOf("keys") + 5;
                    var keys = input.Substring(keysIndex);
                    var keysList = keys.Split(new string[] { "\u0001", "\\lid\\1\\final\\", "final", "\\", "lid" }, StringSplitOptions.RemoveEmptyEntries );

                    var keysResult = new StringBuilder();


                    //var ks = keysList.Aggregate((x, y) => x+" "+y);
                    var timeInSeconds = (ulong)((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);


                    for (int i = 0; i < keysList.Length; i++)
                    {
                        var key = keysList[i];

                        keysResult.Append("\\"+key+"\\");
                        
                        switch (key)
                        {
                            case "points": keysResult.Append("2500"); break;
                            case "points2": keysResult.Append("2000"); break;
                            case "points3": keysResult.Append("1500"); break;
                            case "stars": keysResult.Append("5"); break;
                            case "games": keysResult.Append("90"); break;
                            case "wins": keysResult.Append("20"); break;
                            case "disconn": keysResult.Append("10"); break;
                            case "a_durat": keysResult.Append("200000"); break;
                            case "m_streak": keysResult.Append("10"); break;
                            case "f_race": keysResult.Append("ork_race"); break;
                            case "SM_wins": keysResult.Append("5"); break;
                            case "Chaos_wins": keysResult.Append("5"); break;
                            case "Ork_wins": keysResult.Append("50"); break;
                            case "Tau_wins": keysResult.Append("5"); break;
                            case "SoB_wins": keysResult.Append("5"); break;
                            case "DE_wins": keysResult.Append("5"); break;
                            case "Eldar_wins": keysResult.Append("5"); break;
                            case "IG_wins": keysResult.Append("5"); break;
                            case "Necron_wins": keysResult.Append("5"); break;
                            case "lsw": keysResult.Append("0"); break;
                            case "rnkd_vics": keysResult.Append("50"); break;
                            case "con_rnkd_vics": keysResult.Append("200"); break;
                            case "team_vics": keysResult.Append("250"); break;
                            case "mdls1": keysResult.Append("260"); break;
                            case "mdls2": keysResult.Append("270"); break;
                            case "rg": keysResult.Append("280"); break;
                            case "pw": keysResult.Append("290"); break;
                            default:
                                keysResult.Append("0");
                                break;
                        }

                    }

                    SendToClient(state, $@"\getpdr\1\lid\1\pid\{pid}\mod\{timeInSeconds}\length\{keys.Length}\data\{keysResult}\final\");


                    goto CONTINUE;
                }
                
                if (input.StartsWith(@"\setpd\"))
                {
                    var pid = input.Substring(12, 8);

                    var lidIndex = input.IndexOf("\\lid\\", StringComparison.OrdinalIgnoreCase);
                    var lid = input.Substring(lidIndex+5, 1);

                    var timeInSeconds = (ulong)((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);

                    SendToClient(state, $@"\setpdr\1\lid\{lid}\pid\{pid}\mod\{timeInSeconds}\final\");

                    goto CONTINUE;
                }
            }
            catch (ObjectDisposedException)
            {
                if (state != null)
                    state.Dispose();
                state = null;
                return;
            }
            catch (SocketException e)
            {
                switch (e.SocketErrorCode)
                {
                    case SocketError.ConnectionReset:
                        if (state != null)
                            state.Dispose();
                        state = null;
                        return;
                    case SocketError.Disconnecting:
                        if (state != null)
                            state.Dispose();
                        state = null;
                        return;
                    default:
                        LogError(Category, "Error receiving data");
                        LogError(Category, String.Format("{0} {1}", e.SocketErrorCode, e));
                        if (state != null)
                            state.Dispose();
                        state = null;
                        return;
                }
            }
            catch (Exception e)
            {
                LogError(Category, "Error receiving data");
                LogError(Category, e.ToString());
            }

            // and we wait for more data...
            CONTINUE: WaitForData(state);
        }

        private unsafe int SendToClient(object abstractState, string message)
        {
            var state = (SocketState)abstractState;

            Log("StatsRESP", message);
            
            var bytesToSend = XorBytes(message, XorKEY, 7);

            SendToClient(ref state, bytesToSend);
            return bytesToSend.Length;
        }

        bool SendToClient(ref SocketState state, byte[] data)
        {
            if (data == null)
                return false;
            
            try
            {
                var res = state.Socket.BeginSend(data, 0, data.Length, SocketFlags.None, OnSent, state);
                return true;
            }
            catch (NullReferenceException)
            {
                return false;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.ConnectionAborted &&
                    e.SocketErrorCode != SocketError.ConnectionReset)
                {
                    LogError(Category, "Error sending data");
                    LogError(Category, String.Format("{0} {1}", e.SocketErrorCode, e));
                }
                
                return false;
            }
        }

        void OnSent(IAsyncResult async)
        {
            SocketState state = (SocketState)async.AsyncState;

            if (state == null || state.Socket == null)
                return;

            try
            {
                var remote = (IPEndPoint)state.Socket.RemoteEndPoint;
                int sent = state.Socket.EndSend(async);
               // Log(Category, String.Format("[{0}] Sent {1} byte response to: {2}:{3}", Category, sent, remote.Address, remote.Port));
            }
            catch (NullReferenceException)
            {
            }
            catch (SocketException e)
            {
                switch (e.SocketErrorCode)
                {
                    case SocketError.ConnectionReset:
                    case SocketError.Disconnecting:
                        return;
                    default:
                        LogError(Category, "Error sending data");
                        LogError(Category, String.Format("{0} {1}", e.SocketErrorCode, e));
                        return;
                }
            }
        }

        byte[] XorBytes(byte[] data, int start, int count, string keystr)
        {
            byte[] key = Encoding.ASCII.GetBytes(keystr);

            for (int i = start; i < count; i++)
                data[i] = (byte)(data[i] ^ key[i % key.Length]);

            return data;
        }

        byte[] XorBytes(string str, string keystr, int lengthOffset = 0)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            byte[] key = Encoding.UTF8.GetBytes(keystr);

            var length = data.Length - lengthOffset;

            for (int i = 0; i < length; i++)
                data[i] = (byte)(data[i] ^ key[i % key.Length]);
            
            return data;
        }

        private class SocketState : IDisposable
        {
            public Socket Socket = null;
            public byte[] Buffer = new byte[8192];
            
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                try
                {
                    if (disposing)
                    {
                        if (Socket != null)
                        {
                            try
                            {
                                Socket.Shutdown(SocketShutdown.Both);
                            }
                            catch (Exception)
                            {
                            }
                            Socket.Close();
                            Socket.Dispose();
                            Socket = null;
                        }
                    }

                    GC.Collect();
                }
                catch (Exception)
                {
                }
            }

            ~SocketState()
            {
                Dispose(false);
            }
        }
    }
}
