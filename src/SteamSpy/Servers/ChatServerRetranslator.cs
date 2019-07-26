using GSMasterServer.Data;
using GSMasterServer.Utils;
using SteamSpy.Utils;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GSMasterServer.Servers
{
    internal class ChatServerRetranslator : Server
    {
        private const string Category = "Chat";

        Thread _thread;

        Socket _serverSocket;
        Socket _newPeerAceptingsocket;
        readonly ManualResetEvent _reset = new ManualResetEvent(false);

        public string ChatNick { get; private set; }

        public static byte[] Gamename;
        public static byte[] Gamekey = null;

        public ChatServerRetranslator(IPAddress address, ushort port)
        {
            _thread = new Thread(StartServer)
            {
                Name = "Chat Socket Thread"
            };

            _thread.Start(new AddressInfo()
            {
                Address = address,
                Port = port
            });
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
                _newPeerAceptingsocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(false, 0));

                _newPeerAceptingsocket.Bind(new IPEndPoint(info.Address, info.Port));
                _newPeerAceptingsocket.Listen(10);

                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = 5000,
                    ReceiveTimeout = 5000,
                    SendBufferSize = 65535,
                    ReceiveBufferSize = 65535
                };

                _serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
                _serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(false, 0));

                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
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
            Socket handler = listener.EndAccept(ar);

            SocketState state = new SocketState()
            {
                Socket = handler
            };

            _serverSocket.BeginConnect(new IPEndPoint(IPAddress.Parse(GameConstants.SERVER_ADDRESS), 6668), OnServerConnect, state);
        }

        private void OnServerConnect(IAsyncResult ar)
        {
            var state = (SocketState)ar.AsyncState;

            _serverSocket.EndConnect(ar);

            WaitForGameData(state);
            WaitForServerData(state);
        }

        private void WaitForGameData(SocketState state)
        {
            Thread.Sleep(10);
            if (state == null || state.Socket == null || !state.Socket.Connected)
                return;

            try
            {
                state.Socket.BeginReceive(state.GameBuffer, 0, state.GameBuffer.Length, SocketFlags.None, OnGameDataReceived, state);
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
        private void WaitForServerData(SocketState state)
        {
            Thread.Sleep(10);
            if (state == null || _serverSocket == null || !_serverSocket.Connected)
                return;

            try
            {
                _serverSocket.BeginReceive(state.ServerBuffer, 0, state.ServerBuffer.Length, SocketFlags.None, OnServerDataReceived, state);
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

        private void OnServerDataReceived(IAsyncResult async)
        {
            SocketState state = (SocketState)async.AsyncState;

            if (state == null || _serverSocket == null || !_serverSocket.Connected || state.Socket == null || !state.Socket.Connected)
                return;

            try
            {
                // receive data from the socket
                int received = _serverSocket.EndReceive(async);

                if (received == 0)
                    return;

                state.Socket.Send(state.ServerBuffer, received, SocketFlags.None);
            }

            catch (Exception e)
            {
                LogError(Category, "Error receiving data");
                LogError(Category, e.ToString());
            }

            // and we wait for more data...
            CONTINUE: WaitForServerData(state);
        }

        private unsafe void OnGameDataReceived(IAsyncResult async)
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

                var buffer = state.GameBuffer;

                using (var ms = new MemoryStream(buffer, 0, received))
                {
                    if (!state.Encoded)
                    {
                        using (var reader = new StreamReader(ms))
                        {
                            var asciValue = reader.ReadToEnd();

                            Log("CHATDATA", asciValue);

                            ms.Position = 0;

                            var line = reader.ReadLine();

                            if (line.StartsWith("CRYPT"))
                            {
                                state.Encoded = true;

                                if (line.Contains("whammer40kdc"))
                                {
                                    Gamename = "whammer40kdc".ToAssciiBytes();
                                    Gamekey = "Ue9v3H".ToAssciiBytes();
                                }

                                if (line.Contains("whamdowfr"))
                                {
                                    Gamename = "whamdowfr".ToAssciiBytes();
                                    Gamekey = "pXL838".ToAssciiBytes();
                                }

                                if (Gamekey == null)
                                {
                                    state.Dispose();
                                    return;
                                }

                                var chall = "0000000000000000".ToAssciiBytes();

                                var clientKey = new ChatCrypt.GDCryptKey();
                                var serverKey = new ChatCrypt.GDCryptKey();

                                fixed (byte* challPtr = chall)
                                {
                                    fixed (byte* gamekeyPtr = Gamekey)
                                    {
                                        ChatCrypt.GSCryptKeyInit(clientKey, challPtr, gamekeyPtr, Gamekey.Length);
                                        ChatCrypt.GSCryptKeyInit(serverKey, challPtr, gamekeyPtr, Gamekey.Length);
                                    }
                                }

                                state.ClientKey = clientKey;
                                state.ServerKey = serverKey;

                                _serverSocket.Send(buffer, received, SocketFlags.None);

                                // SendToClient(ref state, DataFunctions.StringToBytes(":s 705 * 0000000000000000 0000000000000000\r\n"));
                            }
                            else
                            {
                                _serverSocket.Send(buffer, received, SocketFlags.None);

                            }
                        }
                    }
                    else
                    {
                        using (var reader = new BinaryReader(ms, Encoding.ASCII))
                        {
                            var start = ms.Position;

                            var bytes = reader.ReadBytes((int)(ms.Length - ms.Position));

                            byte* bytesPtr = stackalloc byte[bytes.Length];

                            for (int i = 0; i < bytes.Length; i++)
                                bytesPtr[i] = bytes[i];

                            ChatCrypt.GSEncodeDecode(state.ClientKey, bytesPtr, bytes.Length);

                            for (int i = 0; i < bytes.Length; i++)
                                bytes[i] = bytesPtr[i];

                            var utf8alue = Encoding.UTF8.GetString(bytes);

                            Log("CHATDATA", utf8alue);

                            if (utf8alue.StartsWith("LOGIN"))
                            {
                                var nick = utf8alue.Split(' ')[2];

                                ChatNick = nick;
                                //var userData = UsersDatabase.Instance.GetUserData(nick);

                                // state.UserInfo = IrcDaemon.RegisterNewUser(state.Socket, nick, userData.ProfileId, state, SendToClient);

                                //var bytesToSend = $":s 707 {nick} 12345678 {userData.ProfileId}\r\n".ToAssciiBytes();

                                //fixed (byte* bytesToSendPtr = bytesToSend)
                                //     ChatCrypt.GSEncodeDecode(state.ServerKey, bytesToSendPtr, bytesToSend.Length);

                                _serverSocket.Send(buffer, received, SocketFlags.None);

                                // SendToClient(ref state, bytesToSend);

                                goto CONTINUE;
                            }

                            if (utf8alue.StartsWith("USRIP"))
                            {
                                /*var remoteEndPoint = ((IPEndPoint)state.Socket.RemoteEndPoint);

                                var bytesToSend = $":s 302  :=+@{remoteEndPoint.Address}\r\n".ToAssciiBytes();

                                fixed (byte* bytesToSendPtr = bytesToSend)
                                    ChatCrypt.GSEncodeDecode(state.ServerKey, bytesToSendPtr, bytesToSend.Length);*/

                                _serverSocket.Send(buffer, received, SocketFlags.None);

                                // SendToClient(ref state, bytesToSend);

                                goto CONTINUE;
                            }

                            if (utf8alue.StartsWith("JOIN"))
                            {
                                // JOIN #GPG!1
                                //JOIN #GSP!whamdowfr!Mllaal1K9M \n\r

                                var channelName = utf8alue.Split(new string[] { " ", "\n\r" }, StringSplitOptions.RemoveEmptyEntries)[1];

                                if (channelName.StartsWith("#GSP!whamdowfr!", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (channelName.Length == 25)
                                    {
                                        var encodedEndPoint = channelName.Substring(15, 10);

                                        // TODO: decode endpoint
                                    }
                                }


                                _serverSocket.Send(buffer, received, SocketFlags.None);
                                goto CONTINUE;
                            }

                            _serverSocket.Send(buffer, received, SocketFlags.None);

                            //IrcDaemon.ProcessSocketMessage(state.UserInfo, utf8alue);
                        }
                    }
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
            CONTINUE: WaitForGameData(state);
        }

       /* private unsafe int SendToClient(object abstractState, string message)
        {
            var state = (SocketState)abstractState;

            if (state.Disposing)
                return 0;

            //Log("CHATRESP", message);

            var bytesToSend = Encoding.UTF8.GetBytes(message);

            if (state.Encoded)
            {
                fixed (byte* bytesToSendPtr = bytesToSend)
                    ChatCrypt.GSEncodeDecode(state.ServerKey, bytesToSendPtr, bytesToSend.Length);
            }

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
                    //LogError(Category, "Error sending data");
                    //LogError(Category, String.Format("{0} {1}", e.SocketErrorCode, e));
                }

                return false;
            }
        }*/

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

        private class SocketState : IDisposable
        {
            public bool Disposing;
            public bool Encoded;
            public Socket Socket = null;
            public byte[] GameBuffer = new byte[8192];
            public byte[] ServerBuffer = new byte[8192];

            public ChatCrypt.GDCryptKey ClientKey;
            public ChatCrypt.GDCryptKey ServerKey;
            //public UserInfo UserInfo;
            public long ProfileId;

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                Disposing = true;
                try
                {
                    if (disposing)
                    {
                        if (Socket != null)
                        {
                            try
                            {
                                Socket.Shutdown(SocketShutdown.Both);
                                //IrcDaemon.RemoveUserFromAllChannels(UserInfo);
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

