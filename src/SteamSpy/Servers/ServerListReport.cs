using GSMasterServer.Data;
using GSMasterServer.Utils;
using ThunderHawk;
using ThunderHawk.Utils;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace GSMasterServer.Servers
{
    internal class ServerListReport : Server
    {
        public const string Category = "ServerReport";
        const int BufferSize = 65535;
        
        Socket _socket;
        SocketAsyncEventArgs _socketReadEvent;
        byte[] _socketReceivedBuffer;

        public static string CurrentUserRoomHash { get; set; }
        
        public ServerListReport(IPAddress listen, ushort port)
        {
            StartServer(new AddressInfo()
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

        ~ServerListReport()
        {
            Dispose(false);
        }

        private void StartServer(AddressInfo info)
        {
            Log(Category, "Starting Server List Reporting");

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

        enum MessageType : byte
        {
            CHALLENGE_RESPONSE = 0x01,
            HEARTBEAT = 0x03,
            KEEPALIVE = 0x08,
            AVAILABLE = 0x09,
            RESPONSE_CORRECT = 0x0A
        }

        private void OnDataReceived(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                IPEndPoint remote = (IPEndPoint)e.RemoteEndPoint;

                byte[] receivedBytes = new byte[e.BytesTransferred];
                Array.Copy(e.Buffer, e.Offset, receivedBytes, 0, e.BytesTransferred);

                var str = Encoding.ASCII.GetString(receivedBytes);

                // there by a bunch of different message formats...
                Log(Category, str);
                
                if (receivedBytes[0] == (byte)MessageType.AVAILABLE)
                {
                    // the initial message is basically the gamename, 0x09 0x00 0x00 0x00 0x00 whamdowfr
                    // reply back a good response
                    byte[] response = new byte[] { 0xfe, 0xfd, 0x09, 0x00, 0x00, 0x00, 0x00 };

                    var resp = Encoding.UTF8.GetString(response);
                    Log(Category, "RESPONCE SERVER CHECK:" + resp);
                    _socket.SendTo(response, remote);
                }
                else if (receivedBytes.Length > 5 && receivedBytes[0] == (byte)MessageType.HEARTBEAT)
                {
                    Log(Category, "========= SEVER DETAILS =========");

                    // this is where server details come in, it starts with 0x03, it happens every 60 seconds or so

                    byte[] uniqueId = new byte[4];
                    Array.Copy(receivedBytes, 1, uniqueId, 0, 4);
                    
                    if (!ParseServerDetails(remote, receivedBytes.Skip(5).ToArray()))
                    {
                        // this should be some sort of proper encrypted challenge, but for now i'm just going to hard code it because I don't know how the encryption works...
                        //byte[] response = new byte[] { 0xfe, 0xfd, 0x01, uniqueId[0], uniqueId[1], uniqueId[2], uniqueId[3], 0x44, 0x3d, 0x73, 0x7e, 0x6a, 0x59, 0x30, 0x30, 0x37, 0x43, 0x39, 0x35, 0x41, 0x42, 0x42, 0x35, 0x37, 0x34, 0x43, 0x43, 0x00 };

                        // Рабочий вариант server challenge из кода сервака для Цивы 4
                        byte[] response = new byte[] { 0xfe, 0xfd, (byte)MessageType.CHALLENGE_RESPONSE, uniqueId[0], uniqueId[1], uniqueId[2], uniqueId[3], 0x41, 0x43, 0x4E, 0x2B, 0x78, 0x38, 0x44, 0x6D, 0x57, 0x49, 0x76, 0x6D, 0x64, 0x5A, 0x41, 0x51, 0x45, 0x37, 0x68, 0x41, 0x00 };

                        Log(Category, "=========!!!! RESPONCE CHANNELGE:" + Encoding.ASCII.GetString(response));

                        _socket.SendTo(response, remote);
                    }
                }
                else if (receivedBytes.Length > 5 && receivedBytes[0] == (byte)MessageType.CHALLENGE_RESPONSE)
                {
                    // this is a challenge response, it starts with 0x01

                    byte[] uniqueId = new byte[4];
                    Array.Copy(receivedBytes, 1, uniqueId, 0, 4);

                    // confirm against the hardcoded challenge
                    //byte[] validate = new byte[] { 0x72, 0x62, 0x75, 0x67, 0x4a, 0x34, 0x34, 0x64, 0x34, 0x7a, 0x2b, 0x66, 0x61, 0x78, 0x30, 0x2f, 0x74, 0x74, 0x56, 0x56, 0x46, 0x64, 0x47, 0x62, 0x4d, 0x7a, 0x38, 0x41, 0x00 };
                    

                    byte[] validate = Encoding.UTF8.GetBytes("Iare43/78WkOVaU1Aanv8vrXbSwA\0"); //new byte[] { 0x41, 0x42, 0x4A, 0x36, 0x47, 0x74, 0x4E, 0x42, 0x35, 0x6D, 0x55, 0x59, 0x48, 0x7A, 0x30, 0x2B, 0x78, 0x34, 0x38, 0x46, 0x36, 0x34, 0x76, 0x4A, 0x54, 0x51, 0x45, 0x41, 0x00 };
                    byte[] validateDC = Encoding.UTF8.GetBytes("Egn4q1jDYyOIVczkXvlGbBxavC4A\0");
                    
                    byte[] clientResponse = new byte[validate.Length];
                    Array.Copy(receivedBytes, 5, clientResponse, 0, clientResponse.Length);

                    var resStr = Encoding.UTF8.GetString(clientResponse);

                    // if we validate, reply back a good response
                    if (clientResponse.SequenceEqual(validate) || clientResponse.SequenceEqual(validateDC))
                    {
                        byte[] response = new byte[] { 0xfe, 0xfd, 0x0a, uniqueId[0], uniqueId[1], uniqueId[2], uniqueId[3] };

                        Log(Category, "==========>>>>> RESPONCE VALIDATION:" + Encoding.ASCII.GetString(response));

                        _socket.SendTo(response, remote);

                        AddValidServer(remote);
                    }
                }
                else if (receivedBytes.Length == 5 && receivedBytes[0] == (byte)MessageType.KEEPALIVE)
                {
                    // this is a server ping, it starts with 0x08, it happens every 20 seconds or so

                    byte[] uniqueId = new byte[4];
                    Array.Copy(receivedBytes, 1, uniqueId, 0, 4);
                    Log(Category, "==========>>>>> REFRESH SERVER PING:" + remote);

                    RefreshServerPing(remote);
                }
            }
            catch (Exception ex)
            {
                LogError(Category, ex.ToString());
            }

            WaitForData();
        }

        private void RefreshServerPing(IPEndPoint remote)
        {
            string key = String.Format("{0}:{1}", remote.Address, remote.Port);
            /*if (Servers.ContainsKey(key))
            {
                GameServer value;
                if (Servers.TryGetValue(key, out value))
                {
                    value["LastPing"] = DateTime.UtcNow;
                    Servers[key] = value;
                }
            }*/
        }

        private bool ParseServerDetails(IPEndPoint remote, byte[] data)
        {
            string receivedData = Encoding.UTF8.GetString(data);

            //Console.WriteLine(receivedData.Replace("\x00", "\\x00").Replace("\x02", "\\x02"));

            // split by 000 (info/player separator) and 002 (players/teams separator)
            // the players/teams separator is really 00, but because 00 may also be used elsewhere (an empty value for example), we hardcode it to 002
            // the 2 is the size of the teams, for BF2 this is always 2.
            string[] sections = receivedData.Split(new string[] { "\x00\x00\x00", "\x00\x00\x02" }, StringSplitOptions.None);

            //Console.WriteLine(sections.Length);

            if (sections.Length != 3 && !receivedData.EndsWith("\x00\x00"))
                return true; // true means we don't send back a response

            string serverVars = sections[0];
            //string playerVars = sections[1];
            //string teamVars = sections[2];

            string[] serverVarsSplit = serverVars.Split(new string[] { "\x00" }, StringSplitOptions.None);

            var server = new GameServer();

            server["IPAddress"] = remote.Address.ToString();
            server["QueryPort"] = remote.Port.ToString();
            server["LastRefreshed"] = DateTime.UtcNow.ToString();
            server["LastPing"] = DateTime.UtcNow.ToString();
            server["country"] = "??";
            
            for (int i = 0; i < serverVarsSplit.Length - 1; i += 2)
            {
                if (serverVarsSplit[i] == "hostname")
                    server.Set(serverVarsSplit[i], Regex.Replace(serverVarsSplit[i + 1], @"\s+", " ").Trim());
                else
                    server.Set(serverVarsSplit[i], serverVarsSplit[i + 1]);
            }

            var gamename = server.Get<string>("gamename");

            if (server.Get<string>("statechanged") == "3" && gamename.Equals("whamdowfram", StringComparison.Ordinal))
            {
                ServerContext.ChatServer.SentServerMessageToClient("Вы создаете хост для игры в авто. Другие игроки увидят ваш хост через некоторое время (до минуты), получат оповещение и смогут подключиться для игры.\n\r");
            }

            server["hostport"] = remote.Port.ToString();
            server["localport"] = remote.Port.ToString();

            var gamevariant = server.Get<string>("gamevariant");

            if (!gamevariant.IsNullOrWhiteSpace())
            {
                if (gamevariant != SteamConstants.GameVariant)
                {
                    ServerContext.ChatServer.SentServerMessageToClient("Вы используете не ту версию модификации. Вам необходимо использовать Soulstorm Bugfix Mod 1.56a.\r\n");
                }
            }

            // you've got to have all these properties in order for your server to be valid
            if (!String.IsNullOrWhiteSpace(server.Get<string>("hostname")) &&
                !String.IsNullOrWhiteSpace(gamevariant) &&
                !String.IsNullOrWhiteSpace(server.Get<string>("gamever")) &&
                !String.IsNullOrWhiteSpace(server.Get<string>("gametype")) &&
                server.Get<string>("maxplayers") != "0")
            {
                server.Valid = true;
            }
            
            // if the server list doesn't contain this server, we need to return false in order to send a challenge
            // if the server replies back with the good challenge, it'll be added in AddValidServer
            if (!SteamLobbyManager.IsInLobbyNow)
                return false;
            
            if (server.Properties.TryGetValue("statechanged", out string value))
            {
                var strValue = value?.ToString();

                if (strValue == "2")
                {
                    SteamLobbyManager.LeaveFromCurrentLobby();
                    return true;
                }
            }

            PortBindingManager.ClearPortBindings();

            var wasJoinable = SteamLobbyManager.IsLobbyJoinable;
            SteamLobbyManager.UpdateCurrentLobby(server);

            if (!wasJoinable && SteamLobbyManager.IsLobbyJoinable)
            {
                var hostname = server.Get<string>("hostname");

                if (gamename == "whamdowfram")
                    ServerContext.ChatServer.SendAutomatchGameBroadcast(hostname, int.Parse(server.Get<string>("maxplayers")));
            }

            return true;
        }
        
        private void AddValidServer(IPEndPoint remote)
        {
            string key = String.Format("{0}:{1}", remote.Address, remote.Port);
            var server = new GameServer();

            server["IPAddress"] = remote.Address.ToString();
            server["QueryPort"] = remote.Port.ToString();
            server["LastRefreshed"] =  DateTime.UtcNow.ToString();
            server["LastPing"] = DateTime.UtcNow.ToString();

            server["hostport"] = remote.Port.ToString();
            server["localport"] = remote.Port.ToString();
            
            SteamLobbyManager.CreatePublicLobby(server, CancellationToken.None);
        }
    }
}
