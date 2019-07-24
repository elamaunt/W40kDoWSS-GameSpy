using GSMasterServer.Data;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Caching;
using System.Threading;

namespace GSMasterServer.Servers
{
    internal class ServerSteamNatNeg : Server
    {
        private const string Category = "NatNegotiation";
        
        public Thread Thread;

        private const int BufferSize = 65535;
        private Socket _socket;
        private SocketAsyncEventArgs _socketReadEvent;
        private byte[] _socketReceivedBuffer;

        private MemoryCache _bindingsCache = new MemoryCache("Steam Bindings");

        public ServerSteamNatNeg(IPAddress listen, ushort port)
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

        ~ServerSteamNatNeg()
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
                        var steamId = reader.ReadUInt64();
                        var bindingId = reader.ReadInt32();
                        var isHost = reader.ReadBoolean();

                        var client = (NatNegSteamClient)_bindingsCache.AddOrGetExisting(new CacheItem(bindingId.ToString(), new NatNegSteamClient(bindingId)), new CacheItemPolicy()
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(15)
                        }).Value;

                        if (isHost)
                        {
                            client.HostPoint = remote;
                            client.Host = new NatNegSteamPeer()
                            {
                                ConnectionId = bindingId,
                                IsHost = isHost,
                                SteamId = steamId
                            };

                            if (client.Guest.HasValue && client.Host.HasValue)
                            {
                                SendResponse(client.HostPoint, client.Guest.Value.Bytes);
                                SendResponse(client.GuestPoint, client.Host.Value.Bytes);
                            }
                        }
                        else
                        {
                            client.GuestPoint = remote;
                            client.Guest = new NatNegSteamPeer()
                            {
                                ConnectionId = bindingId,
                                IsHost = isHost,
                                SteamId = steamId
                            };

                            if (client.Guest.HasValue && client.Host.HasValue)
                            {
                                SendResponse(client.HostPoint, client.Guest.Value.Bytes);
                                SendResponse(client.GuestPoint, client.Host.Value.Bytes);
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

        private void SendResponse(IPEndPoint remote, byte[] bytes)
        {
            _socket.SendTo(bytes, remote);
        }
    }
}
