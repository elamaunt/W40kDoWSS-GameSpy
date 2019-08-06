using GSMasterServer.Data;
using GSMasterServer.Utils;
using SteamSpy.Utils;
using Steamworks;
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
        
        Socket _serverSocket;
        Socket _newPeerAceptingsocket;

        public string ChatNick { get; private set; }
        public readonly int[] ChatRoomPlayersCounts = new int[10];

        public static byte[] Gamename;
        public static byte[] Gamekey = null;
        
        SocketState _currentClientState;

        public ChatServerRetranslator(IPAddress address, ushort port)
        {
            StartServer(new AddressInfo()
            {
                Address = address,
                Port = port
            });
        }

        private void StartServer(AddressInfo info)
        {
            Log(Category, "Init Chat Retranslator");

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
            }
            catch (Exception e)
            {
                LogError(Category, String.Format("Unable to bind Server List Retrieval to {0}:{1}", info.Address, info.Port));
                LogError(Category, e.ToString());
                return;
            }

            RestartAcepting();
        }

        private void RestartAcepting()
        {
            _newPeerAceptingsocket.BeginAccept(AcceptCallback, _newPeerAceptingsocket);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            SocketState state = new SocketState()
            {
                GameSocket = handler,
                ServerSocket = _serverSocket
            };

            _currentClientState = state;

            RestartServerSocket(state);
            
            RestartAcepting();
        }

        private void RestartServerSocket(SocketState state)
        {
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
            if (state == null || state.GameSocket == null || !state.GameSocket.Connected)
                return;

            try
            {
                state.GameSocket.BeginReceive(state.GameBuffer, 0, state.GameBuffer.Length, SocketFlags.None, OnGameDataReceived, state);
            }
            catch (ObjectDisposedException)
            {
                state.GameSocket = null;
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

        private unsafe void OnServerDataReceived(IAsyncResult async)
        {
            SocketState state = (SocketState)async.AsyncState;

            if (state == null || _serverSocket == null || !_serverSocket.Connected || state.GameSocket == null || !state.GameSocket.Connected)
                return;

            try
            {
                // receive data from the socket
                int received = _serverSocket.EndReceive(async);

                if (received == 0)
                    return;
                
                var bytes = new byte[received];

                for (int i = 0; i < received; i++)
                    bytes[i] = state.ServerBuffer[i];
                
                if (state.ReceivingEncoded)
                {
                    fixed (byte* bytesToSendPtr = bytes)
                        ChatCrypt.GSEncodeDecode(state.ReceivingServerKey, bytesToSendPtr, received);
                }
                
                var utf8value = Encoding.UTF8.GetString(bytes);

                Log(Category, utf8value);

                if (utf8value.StartsWith(":s 705", StringComparison.OrdinalIgnoreCase))
                {
                    SendToGameSocket(ref state, bytes);
                    
                    state.ReceivingEncoded = true;
                    goto CONTINUE;
                }

                if (utf8value.IndexOf($@"UTM #GSP!whamdowfr!", StringComparison.OrdinalIgnoreCase) != -1)
                    ProcessHelper.RestoreGameWindow();

                if (utf8value.StartsWith("ROOMCOUNTERS", StringComparison.OrdinalIgnoreCase))
                {
                    var values = utf8value.Split(new string[] { "ROOMCOUNTERS", " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < ChatRoomPlayersCounts.Length; i++)
                        ChatRoomPlayersCounts[i] = 0;

                    for (int i = 0; i < values.Length; i += 2)
                    {
                        var roomIndex = values[i];
                        var count = values[i + 1];

                        ChatRoomPlayersCounts[int.Parse(roomIndex) - 1] = int.Parse(count);
                    }
                    goto CONTINUE;
                }

                var index = utf8value.IndexOf("#GSP!whamdowfr!", StringComparison.OrdinalIgnoreCase);

                if (index != -1)
                {
                    int endIndex = index + 15;
                    for (; endIndex < utf8value.Length; endIndex++)
                    {
                        if (!char.IsDigit(utf8value[endIndex]))
                            break;
                    }

                    var stringSteamId = utf8value.Substring(index + 15, endIndex - index - 15);
                    var steamId = new CSteamID(ulong.Parse(stringSteamId));
                    
                    if (steamId != SteamUser.GetSteamID())
                    {
                        if (ServerListRetrieve.ChannelByIDCache.TryGetValue(steamId, out string roomHash))
                            utf8value = utf8value.Replace(stringSteamId, ServerListRetrieve.ChannelByIDCache[steamId]);
                        else
                            utf8value = utf8value.Replace(stringSteamId, ServerListReport.CurrentUserRoomHash);
                    }
                    else
                        utf8value = utf8value.Replace(stringSteamId, ServerListReport.CurrentUserRoomHash);
                    
                    SendToGameSocket(ref state, Encoding.UTF8.GetBytes(utf8value));

                    goto CONTINUE;
                }

                SendToGameSocket(ref state, bytes);
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

            if (state == null || state.GameSocket == null || !state.GameSocket.Connected)
                return;

            try
            {
                // receive data from the socket
                int received = state.GameSocket.EndReceive(async);

                if (received == 0)
                    return;

                var buffer = state.GameBuffer;

                using (var ms = new MemoryStream(buffer, 0, received))
                {
                    if (!state.SendingEncoded)
                    {
                        using (var reader = new StreamReader(ms))
                        {
                            var asciValue = reader.ReadToEnd();

                            Log("CHATDATA", asciValue);

                            ms.Position = 0;

                            var line = reader.ReadLine();

                            if (line.StartsWith("CRYPT"))
                            {
                                state.SendingEncoded = true;

                                /*if (line.Contains("whammer40kdc"))
                                {
                                    Gamename = "whammer40kdc".ToAssciiBytes();
                                    Gamekey = "Ue9v3H".ToAssciiBytes();
                                }*/

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

                                var receivingGameKey = new ChatCrypt.GDCryptKey();
                                var sendingGameKey = new ChatCrypt.GDCryptKey();
                                var receivingServerKey = new ChatCrypt.GDCryptKey();
                                var sendingServerKey = new ChatCrypt.GDCryptKey();

                                fixed (byte* challPtr = chall)
                                {
                                    fixed (byte* gamekeyPtr = Gamekey)
                                    {
                                        ChatCrypt.GSCryptKeyInit(receivingGameKey, challPtr, gamekeyPtr, Gamekey.Length);
                                        ChatCrypt.GSCryptKeyInit(sendingGameKey, challPtr, gamekeyPtr, Gamekey.Length);
                                        ChatCrypt.GSCryptKeyInit(receivingServerKey, challPtr, gamekeyPtr, Gamekey.Length);
                                        ChatCrypt.GSCryptKeyInit(sendingServerKey, challPtr, gamekeyPtr, Gamekey.Length);
                                    }
                                }

                                state.ReceivingGameKey = receivingGameKey;
                                state.SendingGameKey = sendingGameKey;
                                state.ReceivingServerKey = receivingServerKey;
                                state.SendingServerKey = sendingServerKey;

                                // Send to server without encoding
                                _serverSocket.Send(buffer, received, SocketFlags.None);
                            }
                            else
                            {
                                // Send to server without encoding
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

                            if (state.SendingEncoded)
                            {
                                byte* bytesPtr = stackalloc byte[bytes.Length];

                                for (int i = 0; i < bytes.Length; i++)
                                    bytesPtr[i] = bytes[i];

                                ChatCrypt.GSEncodeDecode(state.ReceivingGameKey, bytesPtr, bytes.Length);

                                for (int i = 0; i < bytes.Length; i++)
                                    bytes[i] = bytesPtr[i];
                            }

                            var utf8value = Encoding.UTF8.GetString(bytes);

                            Log("CHATDATA", utf8value);
                            
                            if (utf8value.StartsWith("LOGIN", StringComparison.OrdinalIgnoreCase))
                            {
                                var nick = utf8value.Split(' ')[2];

                                ChatNick = nick;

                                SendToServerSocket(ref state, bytes);

                                goto CONTINUE;
                            }
                            
                            if (utf8value.StartsWith("USRIP", StringComparison.OrdinalIgnoreCase))
                            {
                                SendToServerSocket(ref state, bytes);

                                goto CONTINUE;
                            }

                            var index = utf8value.IndexOf("#GSP!whamdowfr!", StringComparison.OrdinalIgnoreCase);

                            if (index != -1)
                            {
                                var encodedEndPoint = utf8value.Substring(index + 15, 10);

                                CSteamID steamId;
                                
                                if (ServerListReport.CurrentUserRoomHash == encodedEndPoint)
                                {
                                    steamId = SteamUser.GetSteamID();
                                }
                                else
                                {
                                    if (!ServerListRetrieve.IDByChannelCache.TryGetValue(encodedEndPoint, out steamId))
                                    {
                                        ServerListReport.CurrentUserRoomHash = encodedEndPoint;
                                        steamId = SteamUser.GetSteamID();
                                    }
                                }

                                utf8value = utf8value.Replace(encodedEndPoint, steamId.m_SteamID.ToString());
                                
                                SendToServerSocket(ref state, Encoding.UTF8.GetBytes(utf8value));

                                goto CONTINUE;
                            }
                            
                            SendToServerSocket(ref state, bytes);
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

        public void SendGPGRoomsCountsRequest()
        {
            var state = _currentClientState;

            if (state.Disposing)
                return;

            SendToServerSocket(ref state, $@"ROOMCOUNTERS".ToAssciiBytes());
        }

        public void SentServerMessageToClient(string message)
        {
            var state = _currentClientState;

            if (state.Disposing || ChatNick.IsNullOrWhiteSpace() || message.IsNullOrWhiteSpace())
                return;

            SendToGameSocket(ref state, $@":SERVER!XaaaaaaaaX|10008@127.0.0.1 PRIVMSG {ChatNick} :{message}".ToUTF8Bytes());
        }

        public void SendGameBroadcast(string message)
        {
            var state = _currentClientState;

            if (state.Disposing)
                return;

            SendToServerSocket(ref state, $@"BROADCAST :{message}".ToUTF8Bytes());
        }

        public void SendAutomatchGameBroadcast(string hostname, int maxPlayers)
        {
            var state = _currentClientState;

            if (state.Disposing)
                return;
            
            SendToServerSocket(ref state, $@"GAMEBROADCAST {hostname} {maxPlayers}".ToUTF8Bytes());
        }

        private unsafe void SendToServerSocket(ref SocketState state, byte[] bytes)
        {
            if (state.Disposing)
                return;

            if (state.SendingEncoded)
            {
                fixed (byte* bytesToSendPtr = bytes)
                    ChatCrypt.GSEncodeDecode(state.SendingServerKey, bytesToSendPtr, bytes.Length);
            }

            _serverSocket.Send(bytes, bytes.Length, SocketFlags.None);
        }

        private unsafe void SendToGameSocket(ref SocketState state, byte[] bytes)
        {
            if (state.Disposing)
                return;

          //  Log(Category, "SERVER RESR: " + Encoding.UTF8.GetString(bytes));

            if (state.ReceivingEncoded)
            {
                fixed (byte* bytesToSendPtr = bytes)
                    ChatCrypt.GSEncodeDecode(state.SendingGameKey, bytesToSendPtr, bytes.Length);
            }

            state.GameSocket.Send(bytes, bytes.Length, SocketFlags.None);
        }

        
        void OnSent(IAsyncResult async)
        {
            SocketState state = (SocketState)async.AsyncState;

            if (state == null || state.GameSocket == null)
                return;

            try
            {
                var remote = (IPEndPoint)state.GameSocket.RemoteEndPoint;
                int sent = state.GameSocket.EndSend(async);
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
            public bool ReceivingEncoded;
            public bool SendingEncoded;
            public Socket GameSocket;
            public Socket ServerSocket;
            public byte[] GameBuffer = new byte[8192];
            public byte[] ServerBuffer = new byte[8192];

            public ChatCrypt.GDCryptKey SendingGameKey;
            public ChatCrypt.GDCryptKey ReceivingGameKey;
            public ChatCrypt.GDCryptKey SendingServerKey;
            public ChatCrypt.GDCryptKey ReceivingServerKey;

            //public UserInfo UserInfo;
            //public long ProfileId;

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
                        if (GameSocket != null)
                        {
                            try
                            {
                                GameSocket.Shutdown(SocketShutdown.Both);
                                //IrcDaemon.RemoveUserFromAllChannels(UserInfo);
                            }
                            catch (Exception)
                            {
                            }
                            GameSocket.Close();
                            GameSocket.Dispose();
                            GameSocket = null;

                            if (ServerSocket.Connected)
                                ServerSocket.Disconnect(true);
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

