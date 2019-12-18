using Steamworks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThunderHawk;
using ThunderHawk.Core;
using ThunderHawk.Utils;

namespace GSMasterServer.Servers
{
    public class ServerSteamPortRetranslator
    {
        public const string Category = "ServerRetranslator";
        const int BufferSize = 65535;

        public CSteamID RemoteUserSteamId { get; set; }

        public GameServerDetails AttachedServer { get; set; }

        Socket _socket;
        SocketAsyncEventArgs _socketReadEvent;
        byte[] _socketReceivedBuffer;

        private UdpClient _idsRetrievingClient;

        static readonly IPEndPoint GameEndPoint = new IPEndPoint(IPAddress.Loopback, 6112);

        public ushort Port { get; private set; }
        public IPEndPoint LocalPoint { get; set; }

        public ServerSteamPortRetranslator(CSteamID userId)
            : this()
        {
            RemoteUserSteamId = userId;
            SteamUserStates.CheckConnection(userId.m_SteamID);
        }

        public ServerSteamPortRetranslator()
        {
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
                        (_idsRetrievingClient as IDisposable)?.Dispose();
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
            AttachedServer = null;
        }

        ~ServerSteamPortRetranslator()
        {
            Dispose(false);
        }

        private void StartServer()
        {
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
                _socket.Bind(new IPEndPoint(IPAddress.Any, 0));

                Port = (ushort)((IPEndPoint)_socket.LocalEndPoint).Port;
                
                _socketReadEvent = new SocketAsyncEventArgs()
                {
                    RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0)
                };

                _socketReceivedBuffer = new byte[BufferSize];
                _socketReadEvent.SetBuffer(_socketReceivedBuffer, 0, BufferSize);
                _socketReadEvent.Completed += OnDataReceived;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
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
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void OnDataReceived(object sender, SocketAsyncEventArgs e)
        {
            if (RemoteUserSteamId == CSteamID.Nil)
                return;

            try
            {
                LocalPoint = e.RemoteEndPoint as IPEndPoint;
                var count = (uint)e.BytesTransferred;

                byte[] receivedBytes = new byte[e.BytesTransferred];
                Array.Copy(e.Buffer, e.Offset, receivedBytes, 0, e.BytesTransferred);

                var str = Encoding.UTF8.GetString(receivedBytes);

                // there by a bunch of different message formats...

                //Log(Category, $">> {RemoteUserSteamId} "+str);
                //Log(Category, ">> BYTES:" + string.Join(" ", receivedBytes.Select(x => x.ToString())));
                
                //Console.WriteLine("SendTo "+ _userId.m_SteamID+" "+ e.BytesTransferred);
                // IPEndPoint remote = (IPEndPoint)e.RemoteEndPoint;

                // "��\0���J���\u0001"
                //  ��\0�z�J���\u0001
                // "254 253 0 247 122 228 74 255 255 255 1"
                // "254 253 0 170 87 26 75 255 255 255 1"
                // Fake host response to speed up connection

                try
                {
                    if (count == 11 &&
                        e.Buffer[0] == 254 &&
                        e.Buffer[1] == 253 &&
                        e.Buffer[2] == 0 &&
                        // e.Buffer[3] == 247 &&
                        // e.Buffer[4] == 122 &&
                        // e.Buffer[5] == 228 &&
                        // e.Buffer[6] == 228 &&
                        e.Buffer[7] == 255 &&
                        e.Buffer[8] == 255 &&
                        e.Buffer[9] == 255 &&
                        e.Buffer[10] == 1)
                    {
                       // Log(Category, ">> REQUEST BYTES:" + string.Join(" ", receivedBytes.Select(x => x.ToString())));

                        var builder = new StringBuilder("\0$��Jsplitnum\0�");

                        AppendServerProperty(builder, "numplayers");
                        AppendServerProperty(builder, "maxplayers");
                        AppendServerProperty(builder, "hostname");
                        AppendServerProperty(builder, "hostport", Port.ToString());
                        AppendServerProperty(builder, "mapname");
                        AppendServerProperty(builder, "password");
                        AppendServerProperty(builder, "gamever");
                        AppendServerProperty(builder, "gametype");
                        AppendServerProperty(builder, "numplayers");
                        AppendServerProperty(builder, "maxplayers");
                        AppendServerProperty(builder, "score_", GetRating(AttachedServer?.MaxPlayers));
                        AppendServerProperty(builder, "teamplay");
                        AppendServerProperty(builder, "gametype");
                        AppendServerProperty(builder, "gamevariant");
                        AppendServerProperty(builder, "groupid");
                        AppendServerProperty(builder, "numobservers");
                        AppendServerProperty(builder, "maxobservers");
                        AppendServerProperty(builder, "modname");
                        AppendServerProperty(builder, "moddisplayname");
                        AppendServerProperty(builder, "modversion");
                        AppendServerProperty(builder, "devmode");

                        for (int i = 0; i < 32; i++)
                        {
                            AppendServerProperty(builder, "gametype" + i);
                        }

                        var hostname = AttachedServer?.GetOrDefault("hostname") ?? string.Empty;
                        builder.Append($"\u0001player_\0\0{hostname}\0\0ping_\0\00\0\0player_\0\0{hostname}\0\0\0\u0002\0");

                        var fakeString = builder.ToString();
                        var bytes = Encoding.UTF8.GetBytes(fakeString);

                        // unique Id
                        bytes[1] = e.Buffer[3];
                        bytes[2] = e.Buffer[4];
                        bytes[3] = e.Buffer[5];
                        bytes[4] = e.Buffer[6];

                        // 172 164 27 75

                        Console.WriteLine("SERVER FAKE " + fakeString);

                        _socket?.SendTo(bytes, bytes.Length, SocketFlags.None, LocalPoint ?? GameEndPoint);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                
                SteamNetworking.SendP2PPacket(RemoteUserSteamId, e.Buffer, count, EP2PSend.k_EP2PSendReliable);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            WaitForData();
        }

        private string GetRating(string maxPlayers)
        {
            var profile = CoreContext.MasterServer.CurrentProfile;

            if (maxPlayers == null)
                return profile?.Score1v1?.ToString() ?? "1000";

            switch (maxPlayers)
            {
                case "8":
                case "6":
                    return profile?.Score3v3?.ToString() ?? "1000";
                case "4":
                    return profile?.Score2v2?.ToString() ?? "1000";
                case "2":
                default:
                    return profile?.Score1v1?.ToString() ?? "1000";
            }
        }

        public void SendToGame(byte[] buffer, uint size)
        {
            try
            {
                var s = (int)size;
                var str = Encoding.UTF8.GetString(buffer, 0, s);
                
               // Log(Category, $"<= {RemoteUserSteamId} :: " + str);

                // there by a bunch of different message formats...
                //Log(Category,"<= BYTES:"+ string.Join(" ", buffer.Where((b,i) => i< size).Select(x => x.ToString())));

                int m = 0;
                if (size > 4 &&
                    buffer[0] == 254 &&
                    buffer[1] == 254 &&
                    buffer[2] == 0 &&
                    buffer[3] == 0)

                  // ((buffer[2] == 0 && buffer[3] == 0) ||
                  //  (buffer[2] == 3 && buffer[3] == 0) || 
                  // (buffer[2] == 4 && buffer[3] == 0)))
                  //(buffer[2] == 5 && buffer[3] == 0) || 
                  //(buffer[2] == 6 && buffer[3] == 0) || 
                  //(buffer[2] == 7 && buffer[3] == 0) || 
                  // (buffer[2] == 8 && buffer[3] == 0)))
                {
                    Console.WriteLine("JOINGAMEDATA " + str);

                    var clone = new byte[s];

                    Array.Copy(buffer, clone, s);

                    //Log(Category, "<= BYTES:" + string.Join(" ", buffer.Where((b, i) => i < size).Select(x => x.ToString())));

                    var bytes = ReplaceIPAdresses(clone);

                    _socket?.SendTo(bytes, s, SocketFlags.None, LocalPoint ?? GameEndPoint);
                    
                    return;
                }

                var index = str.IndexOf("hostport", StringComparison.Ordinal);

                if (index != -1)
                {
                    Console.WriteLine("INCOME " + str);
                    // \0#\u001a�\u0001splitnum\0�\0numplayers\01\0maxplayers\02\0hostname\0Bambochuk2\0hostport\063181\0mapname\0\0password\00\0gamever\01.2.120R\0numplayers\01\0maxplayers\02\0score_\0teamplay\00\0gametype\0ranked\0gamevariant\01.56bugfix\0groupid\00\0numobservers\00\0maxobservers\00\0modname\0\0moddisplayname\0\0modversion\0\0devmode\00\0gametype0\0gametype1\0gametype2\0gametype3\0gametype4\0gametype5\0gametype6\0gametype7\0gametype8\0gametype9\0gametype10\0gametype11\0gametype12\0gametype13\0gametype14\0gametype15\0gametype16\0gametype17\0gametype18\0gametype19\0gametype20\0gametype21\0gametype22\0gametype23\0gametype24\0gametype25\0gametype26\0gametype27\0gametype28\0gametype29\0gametype30\0gametype31\0\0\u0001player_\0\0Bambochuk2\0\0ping_\0\00\0\0player_\0\0Bambochuk2\0\0\0\u0002\0"
                    var newStr = str
                        .Replace("score_\0", $"score_\0{GetRating(AttachedServer?.MaxPlayers)}\0")
                        .Replace("hostport\06112", "hostport\0" + Port.ToString())
                        .Replace("hostport\00", "hostport\0"+ Port.ToString());



                    var bytes = Encoding.UTF8.GetBytes(newStr);
                    

                    Console.WriteLine("CONNECTING " + newStr);

                    _socket?.SendTo(bytes, bytes.Length, SocketFlags.None, LocalPoint ?? GameEndPoint);

                    return;
                }
                
                if (_socket == null)
                    Console.WriteLine("RECEIVE SOCKET NULL");
                
                _socket?.SendTo(buffer, (int)size, SocketFlags.None, LocalPoint ?? GameEndPoint);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void AppendServerProperty(StringBuilder builder, string name, string value)
        {
            builder.Append("\0");
            builder.Append(name);
            builder.Append("\0");
            builder.Append(value);
        }

        private void AppendServerProperty( StringBuilder builder, string name)
        {
            builder.Append("\0");
            builder.Append(name);
            builder.Append("\0");
            builder.Append(AttachedServer?.GetOrDefault(name)?? string.Empty);
        }

        private byte[] ReplaceIPAdresses(byte[] bytes)
        {
            // skip start information

            for (int k = 10; k < bytes.Length - 3; k++)
            {
                if (bytes[k] == 'K' &&
                    bytes[k + 1] == '0' &&
                    bytes[k + 2] == '4')
                {
                   if (bytes[k + 3] != 'W')
                        k--;

                    var nickLength = bytes[k + 4];

                    var nickStart = k + 4 + 3;
                    var nickEnd = nickStart + (nickLength << 1);

                    var nick = GetUnicodeString(bytes, nickStart, nickEnd);

                    var stats = CoreContext.MasterServer.GetStatsInfo(nick);

                    var id = new CSteamID(stats.SteamId);

                    var pointStart = nickEnd + 7;

                    bytes[pointStart++] = 127;
                    bytes[pointStart++] = 0;
                    bytes[pointStart++] = 0;
                    bytes[pointStart++] = 1;

                    var port = PortBindingManager.AddOrUpdatePortBinding(id).Port;
                    var portBytes = BitConverter.IsLittleEndian ? BitConverter.GetBytes(port).Reverse().ToArray() : BitConverter.GetBytes(port);

                    bytes[pointStart++] = portBytes[1];
                    bytes[pointStart++] = portBytes[0];

                    k += nickLength + 4 + 6;
                }
            }

            /* OLD
            for (int k = 10; k < bytes.Length - 3; k++)
            {
                if (bytes[k] == 'K' &&
                    bytes[k + 1] == '0' &&
                    bytes[k + 2] == '4')
                {
                    if (bytes[k + 3] != 'W')
                        k--;

                    var nickLength = bytes[k + 4];

                    var nickStart = k + 4 + 3;
                    var nickEnd = nickStart + (nickLength << 1);

                    var nick = GetUnicodeString(bytes, nickStart, nickEnd);

                    var stats = CoreContext.MasterServer.GetStatsInfo(nick);

                    var id = new CSteamID(stats.SteamId);

                    var pointStart = nickEnd + 7;

                    bytes[pointStart++] = 127;
                    bytes[pointStart++] = 0;
                    bytes[pointStart++] = 0;
                    bytes[pointStart++] = 1;

                    var port = PortBindingManager.AddOrUpdatePortBinding(id).Port;
                    var portBytes = BitConverter.IsLittleEndian ? BitConverter.GetBytes(port).Reverse().ToArray() : BitConverter.GetBytes(port);

                    bytes[pointStart++] = portBytes[1];
                    bytes[pointStart++] = portBytes[0];

                    k += nickLength + 4 + 6;
                }
            }
            */
            return bytes;
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
