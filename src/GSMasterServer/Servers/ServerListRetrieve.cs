extern alias reality;

using GSMasterServer.Data;
using IrcD.Channel;
using reality::Reality.Net.Extensions;
using reality::Reality.Net.GameSpy.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using GSMasterServer.Services;
using NLog.Fluent;
using IrcNet.Tools;

namespace GSMasterServer.Servers
{
    internal class ServerListRetrieve
    {
        private const string Category = "ServerRetrieve";

        Thread _thread;
        Socket _socket;
        readonly ServerListReport _report;
        readonly ManualResetEvent _reset = new ManualResetEvent(false);

        public ServerListRetrieve(IPAddress listen, ushort port, ServerListReport report)
        {
            _report = report;
            
			IQueryable<GameServer> servers = _report.Servers.Select(x => x.Value).AsQueryable();

            _thread = new Thread(StartServer)
            {
                Name = "Server Retrieving Socket Thread"
            };

            _thread.Start(new AddressInfo()
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

        ~ServerListRetrieve()
        {
            Dispose(false);
        }

        private void StartServer(object parameter)
        {
            AddressInfo info = (AddressInfo)parameter;

            Logger.Info("Starting Server List Retrieval");

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
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(false, 0));

                _socket.Bind(new IPEndPoint(info.Address, info.Port));
                _socket.Listen(10);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Unable to bind Server List Retrieval to {info.Address}:{info.Port}");
                return;
            }

            while (true)
            {
                _reset.Reset();
                _socket.BeginAccept(AcceptCallback, _socket);
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

                Logger.Error(e, $"Error receiving data. SocketErrorCode: {e.SocketErrorCode}");
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

                Logger.Trace($"Data received: {receivedString}");
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
                        Logger.Error(e, $"Error receiving data. SocketErrorCode: {e.SocketErrorCode}");
                        if (state != null)
                            state.Dispose();
                        state = null;
                        return;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Error receiving data");
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
                Logger.Error(e, $"Error sending data. SocketErrorCode: {e.SocketErrorCode}");
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
                Logger.Trace($"Sent {sent} byte response to: {((IPEndPoint)state.Socket.RemoteEndPoint).Address}:{((IPEndPoint)state.Socket.RemoteEndPoint).Port}");
            }
            catch (SocketException e)
            {
                switch (e.SocketErrorCode)
                {
                    case SocketError.ConnectionReset:
                    case SocketError.Disconnecting:
                        return;
                    default:
                        Logger.Error(e, $"Error sending data. SocketErrorCode: {e.SocketErrorCode}");
                        return;
                }
            }
            finally
            {
                state.Dispose();
                state = null;
            }
        }

        private bool ParseRequest(SocketState state, string message)
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
                    return true;
                }
            }

            var servers =  _report.Servers.Values.Where(x => x.Valid).AsQueryable();

            string[] fields = data[5].Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            // May be real chat rooms:

            /*for (int i = 1; i <= 10; i++)
            {
                SendToClient(state, $@"\{i}\Room {i}\1\200\0\0\password\other\final\".ToAssciiBytes());
            }*/

            // From bf2 server
            /*string gamename = data[1].ToLowerInvariant();
            string validate = data[2].Substring(0, 8);
            string filter = FixFilter(data[2].Substring(8));
            string[] fields = data[3].Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);*/

            //Log(Category, String.Format("Received client request: {0}:{1}", ((IPEndPoint)state.Socket.RemoteEndPoint).Address, ((IPEndPoint)state.Socket.RemoteEndPoint).Port));


           // var server = JsonConvert.SerializeObject((object)_report.Servers.Values.FirstOrDefault() ?? "");

            /*if (!String.IsNullOrWhiteSpace(filter))
            {
                try
                {
                    //Console.WriteLine(filter);
                    servers = servers.Where(filter);
                    //Console.WriteLine(servers.Count());
                }
                catch (Exception e)
                {
                    LogError(Category, "Error parsing filter");
                    LogError(Category, filter);
                    LogError(Category, e.ToString());
                }
            }*/

            // http://aluigi.altervista.org/papers/gslist.cfg
            /*byte[] key;
                key = DataFunctions.StringToBytes("hW6m9a");
            else if (gamename == "arma2oapc")
                key = DataFunctions.StringToBytes("sGKWik");
            else
                key = DataFunctions.StringToBytes("Xn221z");*/

            byte[] unencryptedServerList = PackServerList(state, servers, fields, isAutomatch);
            byte[] encryptedServerList = GSEncoding.Encode(ChatServer.Gamekey, DataFunctions.StringToBytes(validate), unencryptedServerList, unencryptedServerList.LongLength);
            SendToClient(state, encryptedServerList);
            return true;
        }

        private void SendAutomatchRooms(SocketState state, string validate, ChannelInfo[] rooms)
        {
            var bytes = new List<byte>();

            // was ip
            IPEndPoint remoteEndPoint = ((IPEndPoint)state.Socket.RemoteEndPoint);
            bytes.AddRange(remoteEndPoint.Address.GetAddressBytes());

            byte[] value2 = BitConverter.GetBytes((ushort)6500);

            bytes.AddRange(BitConverter.IsLittleEndian ? value2.Reverse() : value2);
            
            bytes.Add(3); // fields count
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

            // #GSP!<gamename>!X<encoded public IP><encoded public port><encoded private IP><encoded private port>X
            // JOIN #GSP!whammer40kdc!MJD13lhaPM
            // X<encoded public IP>X|<profile ID>

            foreach (var room in rooms)
            {
                var encodedEndPoint = room.Name.Split('!')[2];



                //var localip0 = server.Get<string>("localip0");
                //var localip1 = server.Get<string>("localip1");
                //var localport = ushort.Parse(server.Get<string>("localport") ?? "0");
                var queryPort = (ushort)6112;//(ushort)server.Get<int>("QueryPort");
                var iPAddress = "192.168.1.31";//server.Get<string>("localip4") ?? server.Get<string>("IPAddress");

                bytes.Add(81);
                bytes.AddRange(IPAddress.Parse(iPAddress).GetAddressBytes());
                bytes.AddRange(BitConverter.IsLittleEndian ? BitConverter.GetBytes(queryPort).Reverse() : BitConverter.GetBytes(queryPort));

                bytes.Add(255);
                bytes.AddRange(DataFunctions.StringToBytes(room.Name));
                bytes.Add(0);

                bytes.Add(255);
                bytes.AddRange(DataFunctions.StringToBytes(room.Users.Count().ToString()));
                bytes.Add(0);

                bytes.Add(255);
                bytes.AddRange(DataFunctions.StringToBytes("2"));
                bytes.Add(0);
            }

            bytes.AddRange(new byte[] { 0, 255, 255, 255, 255 });

            var array = bytes.ToArray();
            byte[] enc = GSEncoding.Encode(ChatServer.Gamekey, DataFunctions.StringToBytes(validate), array, array.LongLength);
            SendToClient(state, enc);
        }

        private void SendRooms(SocketState state, string validate)
        {
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

           // if (rooms == null)
           // {
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
                    bytes.AddRange(DataFunctions.StringToBytes(ChatServer.IrcDaemon.GetChannelUsersCount("#GPG!" + i).ToString()));
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
            /*}
            else
            {
                for (int i = 0; i < rooms.Length; i++)
                {
                    var room = rooms[i];

                    bytes.Add(81);

                    var b2 = BitConverter.GetBytes((long)i);

                    bytes.Add(b2[3]);
                    bytes.Add(b2[2]);
                    bytes.Add(b2[1]);
                    bytes.Add(b2[0]);

                    bytes.Add(0);
                    bytes.Add(0);

                    bytes.Add(255);
                    bytes.AddRange(DataFunctions.StringToBytes(room.Name));
                    bytes.Add(0);

                    bytes.Add(255);
                    bytes.AddRange(DataFunctions.StringToBytes(room.Users.Count().ToString()));
                    bytes.Add(0);

                    bytes.Add(255);
                    bytes.AddRange(DataFunctions.StringToBytes("2"));
                    bytes.Add(0);

                    bytes.Add(255);
                    bytes.AddRange(DataFunctions.StringToBytes("1"));
                    bytes.Add(0);

                    bytes.Add(255);
                    bytes.AddRange(DataFunctions.StringToBytes("20"));
                    bytes.Add(0);
                }
            }*/

            bytes.AddRange(new byte[] { 0, 255, 255, 255, 255 });

            var array = bytes.ToArray();
            
            //  var array = DataFunctions.StringToBytes("\u007f \0 \0 \u0001 \u0019 d \u0001 \0 hostname \0 \0 Q \u007f \0 \0 \u0001 \u0019 d ÿ Room 1 \0 \0 ÿÿÿÿ");

            // working with Room 1
            //var array = DataFunctions.StringToBytes("\u007f\0\0\u0001\u0019d\u0001\0hostname\0\0Q\u007f\0\0\u0001\u0019dÿRoom 1\0\0ÿÿÿÿ");

            byte[] enc = GSEncoding.Encode(ChatServer.Gamekey, DataFunctions.StringToBytes(validate), array, array.LongLength);

            SendToClient(state, enc);
        }

        private static byte[] PackServerList(SocketState state, IEnumerable<GameServer> servers, string[] fields, bool isAutomatch)
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

            foreach (var server in servers)
            {
                // commented this stuff out since it caused some issues on testing, might come back to it later and see what's happening...
                // NAT traversal stuff...
                // 126 (\x7E)	= public ip / public port / private ip / private port / icmp ip
                // 115 (\x73)	= public ip / public port / private ip / private port
                // 85 (\x55)	= public ip / public port
                // 81 (\x51)	= public ip / public port

                /*var localip1 = server.Get<string>("localip1");
                var localip0 = server.Get<string>("localip0");
                var localport = ushort.Parse(server.Get<string>("localport") ?? "0");
                var queryPort = (ushort)server.Get<int>("QueryPort");
                var iPAddress = server.Get<string>("IPAddress");
                var publicIp = server.Get<string>("publicip");
                var bytes = Encoding.UTF8.GetBytes(publicIp);*/

                // 1388419870 1294854161
                var iPAddress = server.Get<string>("IPAddress");
                var localport = ushort.Parse(server.Get<string>("localport") ?? "0");
                var queryPort = (ushort)server.Get<int>("QueryPort");
                var privateIp = server.Get<string>("localip0");

                /*  if (!String.IsNullOrWhiteSpace(localip0) && !String.IsNullOrWhiteSpace(localip1) && localport > 0)
                  {
                      data.Add(126);
                      data.AddRange(IPAddress.Parse(iPAddress).GetAddressBytes());
                      data.AddRange(BitConverter.IsLittleEndian ? BitConverter.GetBytes((ushort)queryPort).Reverse() : BitConverter.GetBytes((ushort)queryPort));
                      data.AddRange(IPAddress.Parse(localip0).GetAddressBytes());
                      data.AddRange(BitConverter.IsLittleEndian ? BitConverter.GetBytes((ushort)localport).Reverse() : BitConverter.GetBytes((ushort)localport));
                      data.AddRange(IPAddress.Parse(localip1).GetAddressBytes());


                  }
                  else*/
               // if (!String.IsNullOrWhiteSpace(privateIp) && localport > 0)
               // {
                    var bytesIp = IPAddress.Parse(iPAddress).GetAddressBytes();
                    var portBytes = BitConverter.IsLittleEndian ? BitConverter.GetBytes(queryPort).Reverse().ToArray() : BitConverter.GetBytes(queryPort);
                    var bytesLocalIp0 = IPAddress.Parse(privateIp).GetAddressBytes();
                    var localPortBytes = BitConverter.IsLittleEndian ? BitConverter.GetBytes(localport).Reverse().ToArray() : BitConverter.GetBytes(localport);

                    data.Add(115);
                    data.AddRange(bytesIp);
                    data.AddRange(portBytes);
                    data.AddRange(bytesLocalIp0);
                    data.AddRange(localPortBytes);
               /* }
                else
                {
                    data.Add(81); // it could be 85 as well, unsure of the difference, but 81 seems more common...
                    data.AddRange(IPAddress.Parse(localIp).GetAddressBytes());
                    data.AddRange(BitConverter.IsLittleEndian ? BitConverter.GetBytes(queryPort).Reverse() : BitConverter.GetBytes(queryPort));
                }*/

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
                Logger.Error(e.ToString());// это к СС-ке, похоже, не относится, ни разу сюда не провалился
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
                Logger.Error(e.ToString());// это к СС-ке, похоже, не относится, ни разу сюда не провалился
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
