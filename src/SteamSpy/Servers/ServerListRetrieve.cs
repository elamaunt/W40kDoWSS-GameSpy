using GSMasterServer.Data;
using GSMasterServer.Utils;
using Reality.Net.Extensions;
using Reality.Net.GameSpy.Servers;
using ThunderHawk;
using ThunderHawk.Data;
using ThunderHawk.Utils;
using Steamworks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GameServer = GSMasterServer.Data.GameServer;
using ThunderHawk.Core;

namespace GSMasterServer.Servers
{
    internal class ServerListRetrieve : Server
    {
        private const string Category = "ServerRetrieve";
        
        Socket _socket;
        Timer _reloadLobbiesTimer;

        private static volatile Task<GameServer[]> _currentLobbiesTask;

        public static ConcurrentDictionary<string, CSteamID> IDByChannelCache { get; } = new ConcurrentDictionary<string, CSteamID>();
        public static ConcurrentDictionary<CSteamID, string> ChannelByIDCache { get; } = new ConcurrentDictionary<CSteamID, string>();
        
        public ServerListRetrieve(IPAddress listen, ushort port)
        {
            StartServer(new AddressInfo()
            {
                Address = listen,
                Port = port
            });
        }

        public void StartReloadingTimer()
        {
            _reloadLobbiesTimer = new Timer(OnReloadRequested, null, 0, 2000);
        }

        private void OnReloadRequested(object state)
        {
            WarmingUpTheGameList();
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

        ~ServerListRetrieve()
        {
            Dispose(false);
        }

        private void StartServer(object parameter)
        {
            AddressInfo info = (AddressInfo)parameter;

            Log(Category, "Starting Server List Retrieval");

            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = 5000,
                    ReceiveTimeout = 5000,
                    SendBufferSize = 65535,
                    ReceiveBufferSize = 65535
                };

                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

                _socket.Bind(new IPEndPoint(info.Address, info.Port));
                _socket.Listen(10);
            }
            catch (Exception e)
            {
                LogError(Category, String.Format("Unable to bind Server List Retrieval to {0}:{1}", info.Address, info.Port));
                LogError(Category, e.ToString());
                return;
            }

            RestartClientAcepting();
        }

        private void RestartClientAcepting()
        {
            _socket.BeginAccept(AcceptCallback, _socket);
        }

        public static void WarmingUpTheGameList()
        {
            LoadLobbies()
                .ContinueWith(task =>
               {
                   if (task.Status != TaskStatus.RanToCompletion)
                       return;

                   var servers = task.Result;

                   for (int i = 0; i < servers.Length; i++)
                   {
                       var server = servers[i];
                       // start connection establishment
                       SteamNetworking.SendP2PPacket(server.HostSteamId, new byte[] { 0 }, 1, EP2PSend.k_EP2PSendReliable, 1);
                   }
               });
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                SocketState state = new SocketState()
                {
                    Socket = handler
                };

                WaitForData(state);
                RestartClientAcepting();
            }
            catch(Exception ex)
            {
                LogError(Category, "Error receiving data");
                LogError(Category, ex.ToString());
            }
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
            catch (Exception ex)
            {
                LogError(Category, "Error receiving data");
                LogError(Category, ex.ToString());
            }
        }

        private void OnDataReceived(IAsyncResult async)
        {
            SocketState state = (SocketState)async.AsyncState;

            if (state == null || state.Socket == null || !state.Socket.Connected)
                return;

            try
            {
                // receive data from the socket
                int received = state.Socket.EndReceive(async);
                if (received == 0)
                {
                    // when EndReceive returns 0, it means the socket on the other end has been shut down.
                    return;
                }

                var receivedString = Encoding.ASCII.GetString(state.Buffer, 0, received);

                Log(Category, receivedString);

                ParseRequest(state, receivedString);
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
            WaitForData(state);
        }

        private void SendToClient(SocketState state, byte[] data)
        {
            if (state == null)
                return;

            if (state.Socket == null || !state.Socket.Connected)
            {
                state.Dispose();
                state = null;
                return;
            }
            
            try
            {
                state.Socket.BeginSend(data, 0, data.Length, SocketFlags.None, OnSent, state);
            }
            catch (SocketException e)
            {
                LogError(Category, "Error sending data");
                LogError(Category, String.Format("{0} {1}", e.SocketErrorCode, e));
            }
        }

        private void OnSent(IAsyncResult async)
        {
            SocketState state = (SocketState)async.AsyncState;

            if (state == null || state.Socket == null)
                return;

            try
            {
                int sent = state.Socket.EndSend(async);
                Log(Category, String.Format("Sent {0} byte response to: {1}:{2}", sent, ((IPEndPoint)state.Socket.RemoteEndPoint).Address, ((IPEndPoint)state.Socket.RemoteEndPoint).Port));
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
            catch (Exception ex)
            {
                LogError(Category, "Error receiving data");
                LogError(Category, ex.ToString());
            }
            finally
            {
                state.Dispose();
                state = null;
            }
        }

        private void ParseRequest(SocketState state, string message)
        {
            // \u0001\u0012\0\u0001\u0003\u0001\0\0\0whamdowfr\0whamdowfr\0.Ts,PRe`(groupid is null) AND (groupid > 0)\0\\hostname\\gamemode\\hostname\\hostport\\mapname\\password\\gamever\\numplayers\\maxplayers\\score_\\teamplay\\gametype\\gamevariant\\groupid\\numobservers\\maxobservers\\modname\\moddisplayname\\modversion\\devmode\0\0\0\0\u0004

            // auto request
            // \03\0\u0001\u0003\u0001\0\0\0whammer40kdcam\0whammer40kdc\0tMU`s.kv\0\0\0\0\0\u0004

            // chat rooms request
            // \0j\0\u0001\u0003\u0001\0\0\0whammer40kdc\0whammer40kdc\0}%D}s)<}\0\\hostname\\numwaiting\\maxwaiting\\numservers\\numplayersname\0\0\0\0

            string[] data = message.Split(new char[] { '\x00' }, StringSplitOptions.RemoveEmptyEntries);

            //if (!data[2].Equals("whamdowfr", StringComparison.OrdinalIgnoreCase))
            //     return false;

            string validate = data[4];
            string filter = null;

            bool isAutomatch = false;

            if (validate.Length > 8)
            {
                filter = validate.Substring(8);
                validate = validate.Substring(0, 8);
            }
            else
            {
                //Log(Category, "ROOMS REQUEST - "+ data[2]);

                isAutomatch = data[2].EndsWith("am");

                if (!isAutomatch)
                {
                    SendRooms(state, validate);
                    return;
                }
            }

            LoadLobbies()
               .ContinueWith(task =>
               {
                   if (task.Status != TaskStatus.RanToCompletion)
                   {
                       Console.WriteLine(task.Exception);
                       return;
                   }

                   var servers = task.Result;
                   
                   string[] fields = data[5].Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                   
                   byte[] unencryptedServerList = PackServerList(state, servers, fields, isAutomatch);
                   byte[] encryptedServerList = GSEncoding.Encode(/*ChatServer.Gamekey,*/ "pXL838".ToAssciiBytes(), DataFunctions.StringToBytes(validate), unencryptedServerList, unencryptedServerList.LongLength);
                   
                   SendToClient(state, encryptedServerList);
               });
        }

        private static async Task<GameServer[]> LoadLobbies()
        {
            if (_currentLobbiesTask != null)
                return await _currentLobbiesTask;
            var servers = await (_currentLobbiesTask = SteamLobbyManager.LoadLobbies());
            _currentLobbiesTask = null;
            return servers;
        }

        private void SendRooms(SocketState state, string validate)
        {
            ServerContext.ChatServer.SendGPGRoomsCountsRequest();

            // d    whamdowfr whamdowfr fkT>_2Cr \hostname\numwaiting\maxwaiting\numservers\numplayersname
            //var bytes = @"\fieldcount\8\groupid\hostname\numplayers\maxwaiting\numwaiting\numservers\password\other\309\Europe\0\50\0\0\0\.maxplayers.0\408\Pros\0\50\0\0\0\.maxplayers.0\254\West Coast 2\0\50\0\0\0\.maxplayers.0\255\West Coast 3\0\50\0\0\0\.maxplayers.0\256\East Coast 1\0\50\0\0\0\.maxplayers.0\257\East Coast 2\0\50\0\0\0\.maxplayers.0\253\West Coast 1\0\50\0\0\0\.maxplayers.0\258\East Coast 3\0\50\0\0\0\.maxplayers.0\407\Newbies\0\50\0\0\0\.maxplayers.0\final\".ToAssciiBytes();


            //127 0 0 1 207 55

            var bytes = new List<byte>();

            // was ip
            IPEndPoint remoteEndPoint = ((IPEndPoint)state.Socket.RemoteEndPoint);
            bytes.AddRange(remoteEndPoint.Address.GetAddressBytes());

            byte[] value2 = BitConverter.GetBytes((ushort)6500);

            bytes.AddRange(BitConverter.IsLittleEndian ? value2.Reverse() : value2);

            bytes.Add(5); // fields count
            bytes.Add(0);

            bytes.AddRange(DataFunctions.StringToBytes("hostname"));
            bytes.Add(0);
            bytes.Add(0);
            bytes.AddRange(DataFunctions.StringToBytes("numwaiting"));
            bytes.Add(0);
            bytes.Add(0);
            bytes.AddRange(DataFunctions.StringToBytes("maxwaiting"));
            bytes.Add(0);
            bytes.Add(0);
            bytes.AddRange(DataFunctions.StringToBytes("numservers"));
            bytes.Add(0);
            bytes.Add(0);
            bytes.AddRange(DataFunctions.StringToBytes("numplayersname"));
            bytes.Add(0);
            bytes.Add(0);
            
            for (int i = 1; i <= 10; i++)
            {
                bytes.Add(81);

                var b2 = BitConverter.GetBytes((long)i);

                bytes.Add(b2[3]);
                bytes.Add(b2[2]);
                bytes.Add(b2[1]);
                bytes.Add(b2[0]);

                bytes.Add(0);
                bytes.Add(0);

                bytes.Add(255);
                bytes.AddRange(DataFunctions.StringToBytes("Room " + i));
                bytes.Add(0);

                bytes.Add(255);
                bytes.AddRange(DataFunctions.StringToBytes( ServerContext.ChatServer.ChatRoomPlayersCounts[i-1].ToString()));
                bytes.Add(0);

                bytes.Add(255);
                bytes.AddRange(DataFunctions.StringToBytes("1000"));
                bytes.Add(0);

                bytes.Add(255);
                bytes.AddRange(DataFunctions.StringToBytes("1"));
                bytes.Add(0);

                bytes.Add(255);
                bytes.AddRange(DataFunctions.StringToBytes("20"));
                bytes.Add(0);
            }

            bytes.AddRange(new byte[] { 0, 255, 255, 255, 255 });

            var array = bytes.ToArray();
            
            //  var array = DataFunctions.StringToBytes("\u007f \0 \0 \u0001 \u0019 d \u0001 \0 hostname \0 \0 Q \u007f \0 \0 \u0001 \u0019 d ÿ Room 1 \0 \0 ÿÿÿÿ");

            // working with Room 1
            //var array = DataFunctions.StringToBytes("\u007f\0\0\u0001\u0019d\u0001\0hostname\0\0Q\u007f\0\0\u0001\u0019dÿRoom 1\0\0ÿÿÿÿ");

            byte[] enc = GSEncoding.Encode(/*ChatServer.Gamekey*/  "pXL838".ToAssciiBytes(), DataFunctions.StringToBytes(validate), array, array.LongLength);

            SendToClient(state, enc);
        }

        private static byte[] PackServerList(SocketState state, IEnumerable<Data.GameServer> servers, string[] fields, bool isAutomatch)
        {
            IPEndPoint remoteEndPoint = ((IPEndPoint)state.Socket.RemoteEndPoint);
            List<byte> data = new List<byte>();

            data.AddRange(remoteEndPoint.Address.GetAddressBytes());

            byte[] value2 = BitConverter.GetBytes((ushort)remoteEndPoint.Port);
            data.AddRange(BitConverter.IsLittleEndian ? value2.Reverse() : value2);

            if (fields.Length == 1 && fields[0] == "\u0004")
                fields = new string[0];

            data.Add((byte)fields.Length);
            data.Add(0);

            foreach (var field in fields)
            {
                data.AddRange(Encoding.UTF8.GetBytes(field));
                data.AddRange(new byte[] { 0, 0 });
            }

            PortBindingManager.ClearPortBindings();

            foreach (var server in servers)
            {
                if (server.Properties.TryGetValue("gamename", out string gamename))
                {
                    if (isAutomatch && gamename != "whamdowfram")
                        continue;

                    if (!isAutomatch && gamename != "whamdowfr")
                        continue;
                }

                // commented this stuff out since it caused some issues on testing, might come back to it later and see what's happening...
                // NAT traversal stuff...
                // 126 (\x7E)	= public ip / public port / private ip / private port / icmp ip
                // 115 (\x73)	= public ip / public port / private ip / private port
                // 85 (\x55)	= public ip / public port
                // 81 (\x51)	= public ip / public port

                var localip0 = server.Get<string>("localip0");
                ushort localport = ushort.Parse(server.Get<string>("localport") ?? "0");
                var queryPort = (ushort)server.Get<int>("QueryPort");
                var iPAddress = server.Get<string>("IPAddress");
                
                var retranslator = PortBindingManager.AddOrUpdatePortBinding(server.HostSteamId);

                retranslator.AttachedServer = server;

                ushort retranslationPort = retranslator.Port;



                var channelHash = ChatCrypt.PiStagingRoomHash("127.0.0.1", "127.0.0.1", retranslationPort);

                // start connection establishment
                SteamNetworking.SendP2PPacket(server.HostSteamId, new byte[] { 0 }, 1, EP2PSend.k_EP2PSendReliable, 1);

                IDByChannelCache[channelHash] = server.HostSteamId;
                ChannelByIDCache[server.HostSteamId] = channelHash;
                
                var retranslationPortBytes = BitConverter.IsLittleEndian ? BitConverter.GetBytes(retranslationPort).Reverse() : BitConverter.GetBytes(retranslationPort);

                server["hostport"] = retranslationPort.ToString();
                server["localport"] = retranslationPort.ToString();

                var flags = ServerFlags.UNSOLICITED_UDP_FLAG |
                    ServerFlags.PRIVATE_IP_FLAG |
                    ServerFlags.NONSTANDARD_PORT_FLAG |
                    ServerFlags.NONSTANDARD_PRIVATE_PORT_FLAG |
                    ServerFlags.HAS_KEYS_FLAG;

                var loopbackIpBytes = IPAddress.Loopback.GetAddressBytes();

                data.Add((byte)flags);
                data.AddRange(loopbackIpBytes);
                data.AddRange(retranslationPortBytes);
                data.AddRange(loopbackIpBytes);
                data.AddRange(retranslationPortBytes);
                
                data.Add(255);

                for (int i = 0; i < fields.Length; i++)
                {
                    var name = fields[i];
                    var f = GetField(server, name);

                    data.AddRange(Encoding.UTF8.GetBytes(f));

                    if (i < fields.Length - 1)
                    {
                        data.Add(0);
                        data.Add(255);
                    }
                }

                data.Add(0);
            }

            data.Add(0);
            data.Add(255);
            data.Add(255);
            data.Add(255);
            data.Add(255);

            return data.ToArray();
        }

        private static string GetField(GameServer server, string fieldName)
        {
            object value = server.GetByName(fieldName);
            if (value == null)
                return string.Empty;
            else if (value is bool)
                return (bool)value ? "1" : "0";
            else
                return value.ToString();
        }
        private string FixFilter(string filter)
        {
            // escape [
            filter = filter.Replace("[", "[[]");

            // fix an issue in the BF2 main menu where filter expressions aren't joined properly
            // i.e. "numplayers > 0gametype like '%gpm_cq%'"
            // becomes "numplayers > 0 && gametype like '%gpm_cq%'"
            try
            {
                filter = FixFilterOperators(filter);
            }
            catch (Exception e)
            {
                LogError(Category, e.ToString());
            }

            // fix quotes inside quotes
            // i.e. hostname like 'flyin' high'
            // becomes hostname like 'flyin_ high'
            try
            {
                filter = FixFilterQuotes(filter);
            }
            catch (Exception e)
            {
                LogError(Category, e.ToString());
            }

            // fix consecutive whitespace
            filter = Regex.Replace(filter, @"\s+", " ").Trim();

            return filter;
        }

        private static string FixFilterOperators(string filter)
        {
            PropertyInfo[] properties = typeof(GameServer).GetProperties();
            List<string> filterableProperties = new List<string>();

            // get all the properties that aren't "[NonFilter]"
            foreach (var property in properties)
            {
                if (property.GetCustomAttributes(false).Any(x => x.GetType().Name == "NonFilterAttribute"))
                    continue;

                filterableProperties.Add(property.Name);
            }

            // go through each property, see if they exist in the filter,
            // and check to see if what's before the property is a logical operator
            // if it is not, then we slap a && before it
            foreach (var property in filterableProperties)
            {
                IEnumerable<int> indexes = filter.IndexesOf(property);
                foreach (var index in indexes)
                {
                    if (index > 0)
                    {
                        int length = 0;
                        bool hasLogical = IsLogical(filter, index, out length, true) || IsOperator(filter, index, out length, true) || IsGroup(filter, index, out length, true);
                        if (!hasLogical)
                        {
                            filter = filter.Insert(index, " && ");
                        }
                    }
                }
            }
            return filter;
        }

        private static string FixFilterQuotes(string filter)
        {
            StringBuilder newFilter = new StringBuilder(filter);

            for (int i = 0; i < filter.Length; i++)
            {
                int length = 0;
                bool isOperator = IsOperator(filter, i, out length);

                if (isOperator)
                {
                    i += length;
                    bool isInsideString = false;
                    for (; i < filter.Length; i++)
                    {
                        if (filter[i] == '\'' || filter[i] == '"')
                        {
                            if (isInsideString)
                            {
                                // check what's after the quote to see if we terminate the string
                                if (i >= filter.Length - 1)
                                {
                                    // end of string
                                    isInsideString = false;
                                    break;
                                }
                                for (int j = i + 1; j < filter.Length; j++)
                                {
                                    // continue along whitespace
                                    if (filter[j] == ' ')
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        // if it's a logical operator, then we terminate
                                        bool op = IsLogical(filter, j, out length);
                                        if (op)
                                        {
                                            isInsideString = false;
                                            j += length;
                                            i = j;
                                        }
                                        break;
                                    }
                                }
                                if (isInsideString)
                                {
                                    // and if we're still inside the string, replace the quote with a wildcard character
                                    newFilter[i] = '_';
                                }
                                continue;
                            }
                            else
                            {
                                isInsideString = true;
                            }
                        }
                    }
                }
            }

            return newFilter.ToString();
        }

        private static bool IsOperator(string filter, int i, out int length, bool previous = false)
        {
            bool isOperator = false;
            length = 0;

            if (i < filter.Length - 1)
            {
                string op = filter.Substring(i - (i >= 2 ? (previous ? 2 : 0) : 0), 1);
                if (op == "=" || op == "<" || op == ">")
                {
                    isOperator = true;
                    length = 1;
                }
            }

            if (!isOperator)
            {
                if (i < filter.Length - 2)
                {
                    string op = filter.Substring(i - (i >= 3 ? (previous ? 3 : 0) : 0), 2);
                    if (op == "==" || op == "!=" || op == "<>" || op == "<=" || op == ">=")
                    {
                        isOperator = true;
                        length = 2;
                    }
                }
            }

            if (!isOperator)
            {
                if (i < filter.Length - 4)
                {
                    string op = filter.Substring(i - (i >= 5 ? (previous ? 5 : 0) : 0), 4);
                    if (op.Equals("like", StringComparison.InvariantCultureIgnoreCase))
                    {
                        isOperator = true;
                        length = 4;
                    }
                }
            }

            if (!isOperator)
            {
                if (i < filter.Length - 8)
                {
                    string op = filter.Substring(i - (i >= 9 ? (previous ? 9 : 0) : 0), 8);
                    if (op.Equals("not like", StringComparison.InvariantCultureIgnoreCase))
                    {
                        isOperator = true;
                        length = 8;
                    }
                }
            }

            return isOperator;
        }

        private static bool IsLogical(string filter, int i, out int length, bool previous = false)
        {
            bool isLogical = false;
            length = 0;

            if (i < filter.Length - 2)
            {
                string op = filter.Substring(i - (i >= 3 ? (previous ? 3 : 0) : 0), 2);
                if (op == "&&" || op == "||" || op.Equals("or", StringComparison.InvariantCultureIgnoreCase))
                {
                    isLogical = true;
                    length = 2;
                }
            }

            if (!isLogical)
            {
                if (i < filter.Length - 3)
                {
                    string op = filter.Substring(i - (i >= 4 ? (previous ? 4 : 0) : 0), 3);
                    if (op.Equals("and", StringComparison.InvariantCultureIgnoreCase))
                    {
                        isLogical = true;
                        length = 3;
                    }
                }
            }

            return isLogical;
        }

        private static bool IsGroup(string filter, int i, out int length, bool previous = false)
        {
            bool isGroup = false;
            length = 0;

            if (i < filter.Length - 1)
            {
                string op = filter.Substring(i - (i >= 2 ? (previous ? 2 : 0) : 0), 1);
                if (op == "(" || op == ")")
                {
                    isGroup = true;
                    length = 1;
                }
                if (!isGroup && previous)
                {
                    op = filter.Substring(i - (i >= 1 ? (previous ? 1 : 0) : 0), 1);
                    if (op == "(" || op == ")")
                    {
                        isGroup = true;
                        length = 1;
                    }
                }
            }

            return isGroup;
        }

        private class SocketState : IDisposable
        {
            public Socket Socket = null;
            public byte[] Buffer = new byte[8192];

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
