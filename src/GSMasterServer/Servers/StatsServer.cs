using GSMasterServer.Data;
using GSMasterServer.Utils;
using IrcD.Core;
using IrcD.Core.Utils;
using System;
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
            
           // socket.Send(XorBytes(@"\auth\\gamename\whamdowfr\response\f11f999a8f79bb080663a697c87adde5\port\0\id\1\final\", XorKEY));
            socket.Send(XorBytes(@"\lc\1\challenge\KNDVKXFQWP\id\1\final\", XorKEY));
           // socket.Send(XorBytes(@"\auth\\gamename\whamdowfr\response\f11f999a8f79bb080663a697c87ad\port\0\id\1\final\", XorKEY));
            // Thread.Sleep(50);
            // socket.Send(XorBytes(@"\newgame\\sesskey\788355045\\challenge\370025987\final\", XorKEY));
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

                var input = Encoding.ASCII.GetString(XorBytes(buffer, 0, received-7, XorKEY), 0, received);

                Log(Category, input);

                if (input.StartsWith(@"\auth\\gamename\whamdowfr\"))
                {
                    SendToClient(state, @"\lc\2\sesskey\51437\proof\24094a7d02f25bd6b9b221bbf9c47900\id\1\final\");

                    goto CONTINUE;
                }

                if (input.StartsWith(@"\authp\\pid\"))
                {
                    // \authp\\pid\87654321\resp\7e2270c581e8daf5a5321ff218953035\lid\1\final\

                    SendToClient(state, @"\pauthr\100000004\lid\1\final\");

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

            var bytesToSend = XorBytes(message, XorKEY);

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

        byte[] XorBytes(string str, string keystr)
        {
            byte[] data = Encoding.ASCII.GetBytes(str);
            byte[] key = Encoding.ASCII.GetBytes(keystr);

            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)(data[i] ^ key[i % key.Length]);

            return data;
        }

        private class SocketState : IDisposable
        {
            public bool Encoded;
            public Socket Socket = null;
            public byte[] Buffer = new byte[8192];

            public ChatCrypt.GDCryptKey ClientKey;
            public ChatCrypt.GDCryptKey ServerKey;
            
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
