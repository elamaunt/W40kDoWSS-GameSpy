using GSMasterServer.Data;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Caching;
using System.Threading;
using NLog.Fluent;

namespace GSMasterServer.Servers
{
    internal class ServerSteamIdsRetrieve
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
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
            logger.Info("Starting Steam Nat Neg Listener");
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
                logger.Error(e, $"Unable to bind Server List Reporting to {info.Address}:{info.Port}");
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
                logger.Error(e, "Error receiving data");
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
                logger.Error(ex, "Error occured in data processing");
            }

            WaitForData();
        }

        private ulong GetSteamIdByNick(string nick)
        {
            var userData = Database.UsersDBInstance.GetProfileByName(nick);

            if (userData == null)
                return 0;

            return userData.SteamId;
        }

        private void SendResponse(IPEndPoint remote, byte[] bytes)
        {
            _socket.SendTo(bytes, remote);
        }
    }
}
