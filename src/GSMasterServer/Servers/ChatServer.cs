using IrcD.Core;
using IrcD.Core.Utils;
using GSMasterServer.Data;
using GSMasterServer.Utils;
using Reality.Net.GameSpy.Servers;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GSMasterServer.Servers
{
    internal class ChatServer : Server
    {
        private const string Category = "Chat";

        Thread _thread;
        Socket _newPeerAceptingsocket;
        readonly ManualResetEvent _reset = new ManualResetEvent(false);
        readonly IrcDaemon _ircDaemon;

        public ChatServer(IPAddress address, ushort port)
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
            Socket handler = listener.EndAccept(ar);

            SocketState state = new SocketState()
            {
                Socket = handler
            };

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
                                
                                byte[] gamename;
                                byte[] gamekey = null;

                                if (line.Contains("whammer40kdc"))
                                {
                                    gamename = "whammer40kdc".ToAssciiBytes();
                                    gamekey = "Ue9v3H".ToAssciiBytes();
                                }

                                if (line.Contains("whamdowfr"))
                                {
                                    gamename = "whamdowfr".ToAssciiBytes();
                                    gamekey = "pXL838".ToAssciiBytes();
                                }

                                if (gamekey == null)
                                {
                                    state.Dispose();
                                    return;
                                }

                                var chall = "0000000000000000".ToAssciiBytes();

                                var clientKey = new ChatCrypt.GDCryptKey();
                                var serverKey = new ChatCrypt.GDCryptKey();

                                fixed (byte* challPtr = chall)
                                {
                                    fixed (byte* gamekeyPtr = gamekey)
                                    {
                                        ChatCrypt.GSCryptKeyInit(clientKey, challPtr, gamekeyPtr, gamekey.Length);
                                        ChatCrypt.GSCryptKeyInit(serverKey, challPtr, gamekeyPtr, gamekey.Length);
                                    }
                                }

                                state.ClientKey = clientKey;
                                state.ServerKey = serverKey;

                                SendToClient(ref state, DataFunctions.StringToBytes(":s 705 * 0000000000000000 0000000000000000\r\n"));
                            }
                            else
                            {
                                _ircDaemon.ProcessSocketMessage(state.Socket, asciValue, state, SendToClient);
                            }
                        }
                    }
                    else
                    {
                        using (var reader = new BinaryReader(ms, Encoding.ASCII))
                        {
                            var start = ms.Position;

                            var bytes = reader.ReadBytes((int)(ms.Length - ms.Position));

                            var asciValueInput = Encoding.ASCII.GetString(bytes);
                            
                            byte* bytesPtr = stackalloc byte[bytes.Length];

                            for (int i = 0; i < bytes.Length; i++)
                                bytesPtr[i] = bytes[i];

                            ChatCrypt.GSEncodeDecode(state.ClientKey, bytesPtr, bytes.Length);

                            for (int i = 0; i < bytes.Length; i++)
                                bytes[i] = bytesPtr[i];

                            var asciValue = Encoding.ASCII.GetString(bytes);

                            Log("CHATDATA", asciValue);

                            if (asciValue.StartsWith("LOGIN"))
                            {
                                var nick = asciValue.Split(' ')[2];

                                _ircDaemon.RegisterNewUser(state.Socket, nick, state, SendToClient);

                                var bytesToSend = ":s 707 * 12345678 87654321\r\n".ToAssciiBytes();
                               
                                fixed (byte* bytesToSendPtr = bytesToSend)
                                    ChatCrypt.GSEncodeDecode(state.ServerKey, bytesToSendPtr, bytesToSend.Length);

                                SendToClient(ref state, bytesToSend);
                                
                                goto CONTINUE;
                            }

                            if (asciValue.StartsWith("USRIP"))
                            {
                                var bytesToSend = ":s 302 sF|elamaunt :sF|elamaunt=+@127.0.0.1\r\n".ToAssciiBytes();

                                fixed (byte* bytesToSendPtr = bytesToSend)
                                    ChatCrypt.GSEncodeDecode(state.ServerKey, bytesToSendPtr, bytesToSend.Length);

                                SendToClient(ref state, bytesToSend);
                                
                                goto CONTINUE;
                            }
                            
                            _ircDaemon.ProcessSocketMessage(state.Socket, asciValue);
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
            CONTINUE: WaitForData(state);
        }

        private unsafe int SendToClient(object abstractState, string message)
        {
            var state = (SocketState)abstractState;

            Log("CHATRESP", message);

            var bytesToSend = message.ToAssciiBytes();

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
