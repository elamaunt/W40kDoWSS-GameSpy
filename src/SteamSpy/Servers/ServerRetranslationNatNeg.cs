using GSMasterServer.Data;
using SteamSpy.Utils;
using Steamworks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GSMasterServer.Servers
{
    internal class ServerRetranslationNatNeg : Server
    {
        private const string Category = "NatNegotiation Retranslation";
        
        public Thread Thread;

        private const int BufferSize = 65535;

        private Socket _serverSocket;
        private byte[] _serverReceivedBuffer;
        private SocketAsyncEventArgs _serverSocketReadEvent;

        private Socket _clientSocket;
        private SocketAsyncEventArgs _clientSocketReadEvent;
        private byte[] _clientReceivedBuffer;

        private ConcurrentDictionary<int, NatNegConnection> _сonnections = new ConcurrentDictionary<int, NatNegConnection>();

        public ServerRetranslationNatNeg(IPAddress listen, ushort port)
        {
            GeoIP.Initialize(Log, Category);

            Thread = new Thread(StartServer)
            {
                Name = "Server NatNeg Retranslation Thread"
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
                    if (_clientSocket != null)
                    {
                        _clientSocket.Close();
                        _clientSocket.Dispose();
                        _clientSocket = null;
                    }
                    
                    if (_serverSocket != null)
                    {
                        _serverSocket.Close();
                        _serverSocket.Dispose();
                        _serverSocket = null;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        ~ServerRetranslationNatNeg()
        {
            Dispose(false);
        }

        private void StartServer(object parameter)
        {
            AddressInfo info = (AddressInfo)parameter;
            Log(Category, "Starting Nat Neg Listener");

            try
            {
                StartSocket(ref _clientSocket, new IPEndPoint(info.Address, info.Port), OnClientDataReceived, out _clientSocketReadEvent, out _clientReceivedBuffer);
                StartSocket(ref _serverSocket, new IPEndPoint(info.Address, 0), OnServerDataReceived, out _serverSocketReadEvent, out _serverReceivedBuffer);
            }
            catch (Exception e)
            {
                LogError(Category, String.Format("Unable to bind Server List Reporting to {0}:{1}", info.Address, info.Port));
                LogError(Category, e.ToString());
                return;
            }

            WaitForClientData();
            WaitForServerData();
        }
        
        private void StartSocket(ref Socket socket, IPEndPoint point, EventHandler<SocketAsyncEventArgs> handler, out SocketAsyncEventArgs @event, out byte[] buffer)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                SendTimeout = 5000,
                ReceiveTimeout = 5000,
                SendBufferSize = BufferSize,
                ReceiveBufferSize = BufferSize
            };

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
            socket.Bind(point);

            @event = new SocketAsyncEventArgs()
            {
                RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0)
            };

            buffer = new byte[BufferSize];
            @event.SetBuffer(buffer, 0, BufferSize);
            @event.Completed += handler;
        }

        private void WaitForServerData()
        {
            try
            {
                if (!_serverSocket.ReceiveFromAsync(_serverSocketReadEvent))
                    OnServerDataReceived(this, _serverSocketReadEvent);
            }
            catch (SocketException e)
            {
                LogError(Category, "Error receiving server data");
                LogError(Category, e.ToString());
                return;
            }
        }

        private void WaitForClientData()
        {
            try
            {
                if (!_clientSocket.ReceiveFromAsync(_clientSocketReadEvent))
                    OnClientDataReceived(this, _clientSocketReadEvent);
            }
            catch (SocketException e)
            {
                LogError(Category, "Error receiving client data");
                LogError(Category, e.ToString());
                return;
            }
        }

        private void OnServerDataReceived(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                IPEndPoint remote = (IPEndPoint)e.RemoteEndPoint;

                using (var ms = new MemoryStream(e.Buffer, e.Offset, e.BytesTransferred))
                {
                    using (var reader = new BinaryReader(ms))
                    {
                        var steamId = new CSteamID(reader.ReadUInt64());
                        var connectionId = reader.ReadInt32();
                        var isHost = reader.ReadBoolean();

                        Log(Category, "Server NAT received "+ steamId.m_SteamID);

                        if (_сonnections.TryGetValue(connectionId, out NatNegConnection connection))
                        {
                            var port = PortBindingManager.AddOrUpdatePortBinding(steamId).Port;

                            var peer = new NatNegPeer()
                            {
                                IsHost = isHost,
                                CommunicationAddress = new IPEndPoint(IPAddress.Loopback, port),
                                PublicAddress = new IPEndPoint(IPAddress.Loopback, port)
                            };

                            if (isHost)
                                connection.Host = peer;
                            else
                                connection.Guest = peer;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(Category, ex.ToString());
            }

            WaitForServerData();
        }

        private void OnClientDataReceived(object sender, SocketAsyncEventArgs e)
        {
            /*
             * Connection Protocol
             * 
             * From http://wiki.tockdom.com/wiki/MKWii_Network_Protocol/Server/mariokartwii.natneg.gs.nintendowifi.net
             * 
             * The NATNEG communication to enable a peer to peer communication is is done in the following steps:
             * 
             * Both clients (called guest and host to distinguish them) exchange an unique natneg-id. In all observed Wii games this communication is done using Server MS and Server MASTER.
             * Both clients sends independent of each other a sequence of 4 INIT packets to the NATNEG servers. The sequence number goes from 0 to 3. The guest sets the host_flag to 0 and the host to 1. The natneg-id must be the same for all packets.
             * Packet 0 (sequence number 0) is send from the public address to server NATNEG1. This public address is later used for the peer to peer communication.
             * Packet 1 (sequence number 1) is send from the communication address (usually an other port than the public address) to server NATNEG1.
             * Packet 2 (sequence number 2) is send from the communication address to server NATNEG2 (any kind of fallback?).
             * Packet 3 (sequence number 3) is send from the communication address to server NATNEG3 (any kind of fallback?).
             * Each INIT packet is answered by an INIT_ACK packet as acknowledge to the original sender.
             * If server NATNEG1 have received all 4 INIT packets with sequence numbers 0 and 1 (same natneg-id), then it sends 2 CONNECT packets:
             * One packet is send to the communication address of the guest. The packet contains the public address of the host as data.
             * The other packet is send to the communication address of the host. The packet contains the public address of the quest as data.
             * Both clients send back a CONNECT_ACK packet to NATNEG1 as acknowledge.
             * Both clients start peer to peer communication using the public addresses.
             * 
             * C implementation:
             * See http://aluigi.altervista.org/papers/gsnatneg.c
             * 
             * Game names and game keys:
             * Civilization IV: Beyond the Sword             civ4bts         Cs2iIq
             * Mario Kart Wii (Wii)                          mariokartwii    9r3Rmy
             * 
             */
            try
            {
                IPEndPoint remote = (IPEndPoint)e.RemoteEndPoint;

                byte[] receivedBytes = new byte[e.BytesTransferred];
                Array.Copy(e.Buffer, e.Offset, receivedBytes, 0, e.BytesTransferred);

                NatNegMessage message = null;
                try
                {
                    message = NatNegMessage.ParseData(receivedBytes);
                }
                catch (Exception ex)
                {
                    LogError(Category, ex.ToString());
                }
                if (message == null)
                {
                    Log(Category, "Received unknown data " + string.Join(" ", receivedBytes.Select((b) => { return b.ToString("X2"); }).ToArray()) + " from " + remote.ToString());
                }
                else
                {
                    //commented out 10.10
                    //Program.LogError('['+ Category+']' + "Received message " + message.ToString() + " from " + remote.ToString());
                    //Log(Category, "(Message bytes: " + string.Join(" ", receivedBytes.Select((b) => { return b.ToString("X2"); }).ToArray()) + ")");
                    if (message.RecordType == 0)
                    {
                        // INIT, return INIT_ACK
                        message.RecordType = 1;
                        SendByClientSocket(remote, message);

                        if (message.SequenceId > 1)
                        {
                            // Messages sent to natneg2 and natneg3, they only require an INIT_ACK. Used by client to determine NAT mapping mode?
                        }
                        else
                        {
                            // Collect data and send CONNECT messages if you have two peers initialized with all necessary data
                            if (!_сonnections.ContainsKey(message.ClientId))
                                _сonnections[message.ClientId] = new NatNegConnection();
                           
                            var client = _сonnections[message.ClientId];
                            
                            client.ConnectionId = message.ClientId;

                            bool isHost = message.IsHost;

                            NatNegPeer peer = isHost ? client.Host : client.Guest;

                            if (peer == null)
                            {
                                peer = new NatNegPeer();

                                if (isHost)
                                    client.Host = peer;
                                else
                                    client.Guest = peer;
                            }

                            peer.IsHost = isHost;

                            if (message.SequenceId == 0)
                                peer.PublicAddress = remote;
                            else
                                peer.CommunicationAddress = remote;
                            
                            if (client.Guest != null && client.Guest.CommunicationAddress != null && client.Guest.PublicAddress != null && client.Host != null && client.Host.CommunicationAddress != null && client.Host.PublicAddress != null)
                            {
                                /* If server NATNEG1 have received all 4 INIT packets with sequence numbers 0 and 1 (same natneg-id), then it sends 2 CONNECT packets:
                                 * One packet is send to the communication address of the guest. The packet contains the public address of the host as data.
                                 * The other packet is send to the communication address of the host. The packet contains the public address of the quest as data.
                                 */

                                // Remove client from dictionary
                                //NatNegClient removed = null;
                                //_Clients.TryRemove(client.ClientId, out removed);

                                message.RecordType = 5;
                                message.Error = 0;
                                message.GotData = 0x42;

                                if (isHost)
                                {
                                    message.ClientPublicIPAddress = NatNegMessage._toIpAddress(client.Guest.PublicAddress.Address.GetAddressBytes());
                                    message.ClientPublicPort = (ushort)client.Guest.PublicAddress.Port;
                                    SendByClientSocket(client.Host.CommunicationAddress, message);
                                }
                                else
                                {
                                    message.ClientPublicIPAddress = NatNegMessage._toIpAddress(client.Host.PublicAddress.Address.GetAddressBytes());
                                    message.ClientPublicPort = (ushort)client.Host.PublicAddress.Port;
                                    SendByClientSocket(client.Guest.CommunicationAddress, message);
                                }
                                //Log(Category, "Sent connect messages to peers with clientId " + client.ClientId + " connecting host " + client.Host.PublicAddress.ToString() + " and guest " + client.Guest.PublicAddress.ToString());
                            }
                            else
                            {
                                using (var ms = new MemoryStream())
                                {
                                    using (var writer = new BinaryWriter(ms))
                                    {
                                        writer.Write(SteamUser.GetSteamID().m_SteamID);
                                        writer.Write(message.ClientId);
                                        writer.Write(isHost);
                                        
                                        SendByServerSocket(new IPEndPoint(IPAddress.Parse("134.209.198.2"), 27902), ms.ToArray());
                                    }
                                }
                            }
                        }
                    }
                    else if (message.RecordType == 13)
                    {
                        // REPORT, return REPORT_ACK
                        message.RecordType = 14;
                        SendByClientSocket(remote, message);
                        //Console.WriteLine("somon natnegd in servernatneg.cs 'else if (message.RecordType == 13)'");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(Category, ex.ToString());
            }

            WaitForClientData();
        }

        private void SendByClientSocket(IPEndPoint remote, NatNegMessage message)
        {
            byte[] response = message.ToBytes();
            _clientSocket.SendTo(response, remote);
        }

        private void SendByServerSocket(IPEndPoint remote, byte[] bytes)
        {
            _serverSocket.SendTo(bytes, remote);
        }
    }
}
