using GSMasterServer.Data;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Caching;
using System.Threading;

namespace GSMasterServer.Servers
{
    internal class ServerSteamIdsRetrieve : Server
    {
        private const string Category = "NatNegotiation";
        
        public Thread Thread;

        private const int BufferSize = 65535;
        private Socket _socket;
        private SocketAsyncEventArgs _socketReadEvent;
        private byte[] _socketReceivedBuffer;
        private byte[] _socketSendBuffer;

        private MemoryCache _steamIdsCache = new MemoryCache("Steam ids Cache");

        public ServerSteamIdsRetrieve(IPAddress listen, ushort port)
        {
            GeoIP.Initialize(Log, Category);

            Thread = new Thread(StartServer)
            {
                Name = "Server NatNeg Socket Thread"
            };
            Thread.Start(new AddressInfo()
            {
                Address = listen,
                Port = port
            });
        }

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
                    if (_socket != null)
                    {
                        _socket.Close();
                        _socket.Dispose();
                        _socket = null;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        ~ServerSteamIdsRetrieve()
        {
            Dispose(false);
        }

        private void StartServer(object parameter)
        {
            AddressInfo info = (AddressInfo)parameter;
            Log(Category, "Starting Steam Nat Neg Listener");
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                {
                    SendTimeout = 5000,
                    ReceiveTimeout = 5000,
                    SendBufferSize = BufferSize,
                    ReceiveBufferSize = BufferSize
                };

                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
                _socket.Bind(new IPEndPoint(info.Address, info.Port));

                _socketReadEvent = new SocketAsyncEventArgs()
                {
                    RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0)
                };

                _socketReceivedBuffer = new byte[BufferSize];
                _socketSendBuffer = new byte[BufferSize];

                _socketReadEvent.SetBuffer(_socketReceivedBuffer, 0, BufferSize);
                _socketReadEvent.Completed += OnDataReceived;
            }
            catch (Exception e)
            {
                LogError(Category, String.Format("Unable to bind Server List Reporting to {0}:{1}", info.Address, info.Port));
                LogError(Category, e.ToString());
                return;
            }

            WaitForData();
        }

        private void WaitForData()
        {
            Thread.Sleep(10);
            GC.Collect();

            try
            {
                if (!_socket.ReceiveFromAsync(_socketReadEvent))
                    OnDataReceived(this, _socketReadEvent);
            }
            catch (SocketException e)
            {
                LogError(Category, "Error receiving data");
                LogError(Category, e.ToString());
                return;
            }
        }

        private void OnDataReceived(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                IPEndPoint remote = (IPEndPoint)e.RemoteEndPoint;

                using (var ms = new MemoryStream(e.Buffer, 0, e.BytesTransferred))
                {
                    using (var reader = new BinaryReader(ms))
                    {
                        using (var sendStream = new MemoryStream(_socketSendBuffer))
                        {
                            using (var writer = new BinaryWriter(sendStream))
                            {
                                while (ms.Position < ms.Length)
                                {
                                    var nick = reader.ReadString();

                                    writer.Write(nick);

                                    ulong steamId = 0;

                                    if (_steamIdsCache.Contains(nick))
                                        steamId = (ulong)_steamIdsCache.Get(nick);
                                    else
                                    {
                                        steamId = GetSteamIdByNick(nick);

                                        if (steamId == 0)
                                            goto CONTINUE;

                                        _steamIdsCache.Add(new CacheItem(nick, steamId), new CacheItemPolicy()
                                        {
                                            SlidingExpiration = TimeSpan.FromMinutes(5)
                                        });
                                    }

                                    writer.Write(steamId);
                                }

                                CONTINUE: _socket.SendTo(_socketSendBuffer, (int)sendStream.Position, SocketFlags.None, remote);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(Category, ex.ToString());
            }

            WaitForData();
        }

        private ulong GetSteamIdByNick(string nick)
        {
            return UsersDatabase.Instance.GetUserData(nick).SteamId;
        }

        private void SendResponse(IPEndPoint remote, byte[] bytes)
        {
            _socket.SendTo(bytes, remote);
        }
    }
}
