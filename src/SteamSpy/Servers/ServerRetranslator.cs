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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GSMasterServer.Servers
{
    public class ServerRetranslator : Server
    {
        public const string Category = "ServerRetranslator";
        const int BufferSize = 65535;

        public CSteamID RemoteUserSteamId { get; set; }

        Socket _socket;
        SocketAsyncEventArgs _socketReadEvent;
        byte[] _socketReceivedBuffer;

        private UdpClient _idsRetrievingClient;

        static readonly IPEndPoint GameEndPoint = new IPEndPoint(IPAddress.Loopback, 6112);

        public ushort Port { get; private set; }
        public IPEndPoint LocalPoint { get; set; }

        static readonly ConcurrentDictionary<string, CSteamID> IdByNicksCache = new ConcurrentDictionary<string, CSteamID>();

        public ServerRetranslator(CSteamID userId)
            : this()
        {
            RemoteUserSteamId = userId;
        }

        public ServerRetranslator()
        {
            GeoIP.Initialize(Log, Category);
            StartServer();
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
                        _socketReadEvent.Completed -= OnDataReceived;
                        _socketReadEvent.Dispose();
                        _socketReadEvent = null;

                        _socket.Close();
                        _socket.Dispose();
                        _socket = null;
                    }

                    if (_idsRetrievingClient != null)
                    {
                        _idsRetrievingClient.Dispose();
                        _idsRetrievingClient = null;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public void Clear()
        {
            LocalPoint = null;
        }

        ~ServerRetranslator()
        {
            Dispose(false);
        }

        private void StartServer()
        {
            Log(Category, "Starting Server Retranslator");

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
                _socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));

                Port = (ushort)((IPEndPoint)_socket.LocalEndPoint).Port;

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
                if (_socket == null)
                    Console.WriteLine("WAIT SOCKET NULL");

                if (!_socket.ReceiveFromAsync(_socketReadEvent))
                    OnDataReceived(this, _socketReadEvent);
            }
            catch(NullReferenceException ex)
            {

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
            if (RemoteUserSteamId == CSteamID.Nil)
                return;

            try
            {
                LocalPoint = e.RemoteEndPoint as IPEndPoint;

                //byte[] receivedBytes = new byte[e.BytesTransferred];
                //Array.Copy(e.Buffer, e.Offset, receivedBytes, 0, e.BytesTransferred);

                //var str = Encoding.UTF8.GetString(receivedBytes);

                // there by a bunch of different message formats...
                //Log(Category, $">> {RemoteUserSteamId} "+str);

                //Console.WriteLine("SendTo "+ _userId.m_SteamID+" "+ e.BytesTransferred);
                // IPEndPoint remote = (IPEndPoint)e.RemoteEndPoint;

                var count = (uint)e.BytesTransferred;

                if (count < 1000)
                    SteamNetworking.SendP2PPacket(RemoteUserSteamId, e.Buffer, count, EP2PSend.k_EP2PSendUnreliableNoDelay);
                else
                    SteamNetworking.SendP2PPacket(RemoteUserSteamId, e.Buffer, count, EP2PSend.k_EP2PSendReliable);
            }
            catch (Exception ex)
            {
                LogError(Category, ex.ToString());
            }

            WaitForData();
        }

        public void Send(byte[] buffer, uint size)
        {
            try
            {
                var s = (int)size;
                //var str = Encoding.UTF8.GetString(buffer, 0, s);
                
                //Log(Category, $"<= {RemoteUserSteamId} :: " + str);

                // there by a bunch of different message formats...
                //Log(Category,"<= BYTES:"+ string.Join(" ", buffer.Where((b,i) => i< size).Select(x => x.ToString())));

                int m = 0;
                if (size > 4 &&
                    buffer[m++] == 254 &&
                    buffer[m++] == 254 &&
                    buffer[m++] == 0 &&
                    buffer[m++] == 0)
                {
                    var clone = new byte[s];

                    Array.Copy(buffer, clone, s);

                    HandleGamelobbyRequest(clone)
                        .ContinueWith(task =>
                        {
                            if (task.Status == TaskStatus.RanToCompletion)
                                _socket?.SendTo(task.Result, s, SocketFlags.None, LocalPoint ?? GameEndPoint);
                            else
                                _socket?.SendTo(buffer, s, SocketFlags.None, LocalPoint ?? GameEndPoint);
                        });
                    
                    return;
                }
                
                if (_socket == null)
                    Console.WriteLine("RECEIVE SOCKET NULL");

                _socket?.SendTo(buffer, (int)size, SocketFlags.None, LocalPoint ?? GameEndPoint);
            }
            catch (Exception ex)
            {
                LogError(Category, ex.ToString());
            }
        }

        private async Task<byte[]> HandleGamelobbyRequest(byte[] bytes)
        {
            var nicks = new List<string>();

            // skip start information
            for (int k = 50; k < bytes.Length - 3; k++)
            {
                if (bytes[k] == 'K' &&
                    bytes[k + 1] == '0' &&
                    bytes[k + 2] == '4' &&
                    bytes[k + 3] == 'W')
                {
                    var nickLength = bytes[k + 4];

                    var nickStart = k + 4 + 3;
                    var nickEnd = nickStart + (nickLength << 1);

                    var nick = GetUnicodeString(bytes, nickStart, nickEnd);

                    if (!IdByNicksCache.ContainsKey(nick))
                        nicks.Add(GetUnicodeString(bytes, nickStart, nickEnd));
                }
            }

            if (nicks.Count > 0)
                await LoadSteamIds(nicks);

            // skip start information
            for (int k = 50; k < bytes.Length - 3; k++)
            {
                if (bytes[k] == 'K' &&
                    bytes[k + 1] == '0' &&
                    bytes[k + 2] == '4' &&
                    bytes[k + 3] == 'W')
                {
                    var nickLength = bytes[k + 4];

                    var nickStart = k + 4 + 3;
                    var nickEnd = nickStart + (nickLength << 1);

                    var nick = GetUnicodeString(bytes, nickStart, nickEnd);

                    if (IdByNicksCache.TryGetValue(nick, out CSteamID id))
                    {
                        var pointStart = nickEnd + 7;

                        bytes[pointStart++] = 127;
                        bytes[pointStart++] = 0;
                        bytes[pointStart++] = 0;
                        bytes[pointStart++] = 1;

                        var port = PortBindingManager.AddOrUpdatePortBinding(id).Port;
                        var portBytes = BitConverter.IsLittleEndian ? BitConverter.GetBytes(port).Reverse().ToArray() : BitConverter.GetBytes(port);

                        bytes[pointStart++] = portBytes[1];
                        bytes[pointStart++] = portBytes[0];
                    }
                    else
                        throw new Exception("Unknown player nick - "+nick);
                }
            }

            return bytes;
        }

        private async Task LoadSteamIds(List<string> nicks)
        {
            try
            {
                var ms = new MemoryStream();
                var writer = new BinaryWriter(ms);

                for (int i = 0; i < nicks.Count; i++)
                    writer.Write(nicks[i]);

                var buffer = ms.GetBuffer();

                var client = _idsRetrievingClient = new UdpClient();

                var endPoint = new IPEndPoint(IPAddress.Parse(GameConstants.SERVER_ADDRESS), GameConstants.IDS_REQUEST_PORT);

                await client.SendAsync(buffer, buffer.Length, endPoint);
                var result = await client.ReceiveAsync();

                ms = new MemoryStream(result.Buffer);
                var reader = new BinaryReader(ms);
                
                while (ms.Position + 1 < ms.Length)
                    IdByNicksCache.TryAdd(reader.ReadString(), new CSteamID(reader.ReadUInt64()));
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR on loading steam ids "+ex);
            }
        }

        private static string GetUnicodeString(byte[] bytes, int index, int index2)
        {
            var bytesClone = new byte[index2 - index];

            for (int i = 0; i < index2 - index; i += 2)
            {
                bytesClone[i] = bytes[index + i + 1];
                bytesClone[i + 1] = bytes[index + i];
            }

            return Encoding.Unicode.GetString(bytesClone);
        }
    }
}
