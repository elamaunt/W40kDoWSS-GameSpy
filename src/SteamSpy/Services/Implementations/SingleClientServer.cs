using Framework;
using GSMasterServer.Utils;
using Http;
using Reality.Net.GameSpy.Servers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using ThunderHawk.Core;
using ThunderHawk.Utils;

namespace ThunderHawk
{
    public class SingleClientServer : IClientServer
    {
        UdpPortHandler _serverReport;
        TcpPortHandler _serverRetrieve;
        TcpPortHandler _clientManager;
        TcpPortHandler _searchManager;
        TcpPortHandler _chat;
        TcpPortHandler _stats;
        TcpPortHandler _http;

        Timer _keepAliveTimer;
        volatile int _heartbeatState;

        string _serverChallenge;
        string _clientChallenge;

        string _passwordEncrypted;
        bool _chatEncoded;

        string _user;
        string _name;
        long _profileId;
        string _email;
        string _response;
        bool _inChat;

        byte[] _gameNameBytes;
        byte[] _gamekeyBytes;

        ChatCrypt.GDCryptKey _chatClientKey;
        ChatCrypt.GDCryptKey _chatServerKey;
        readonly char[] ChatSplitChars = new[] { '\r', '\n' };
        readonly char[] ChatCommandsSplitChars = new[] { ' ' };
        enum MessageType : byte
        {
            CHALLENGE_RESPONSE = 0x01,
            HEARTBEAT = 0x03,
            KEEPALIVE = 0x08,
            AVAILABLE = 0x09,
            RESPONSE_CORRECT = 0x0A
        }

        public SingleClientServer()
        {
            _serverReport = new UdpPortHandler(27900, OnServerReport, OnError);
            _serverRetrieve = new TcpPortHandler(28910, OnServerRetrieve, OnError, null, OnZeroBytes);

            _clientManager = new TcpPortHandler(29900, OnClientManager, OnError, OnClientAccept, OnZeroBytes);
            _searchManager = new TcpPortHandler(29901, OnSearchManager, OnError, null, OnZeroBytes);

            _chat = new TcpPortHandler(6667, OnChat, OnError, OnChatAccept, OnZeroBytes);
            _stats = new TcpPortHandler(29920, OnStats, OnError, null, OnZeroBytes);
            _http = new TcpPortHandler(80, OnHttp, OnError, null, OnZeroBytes);

            CoreContext.MasterServer.LoginInfoReceived += SendLoginResponce;
        }

        void OnZeroBytes(TcpPortHandler handler)
        {
            Restart();
        }

        void OnClientAccept(TcpPortHandler handler, TcpClient client, CancellationToken token)
        {
            //Обновляем челендж для нового соединения
            _serverChallenge = RandomHelper.GetString(10);
            handler.Send(DataFunctions.StringToBytes($@"\lc\1\challenge\{_serverChallenge}\id\1\final\"));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Start()
        {
            _serverReport.Start();
            _serverRetrieve.Start();

            _clientManager.Start();
            _searchManager.Start();

            _chat.Start();
           // _stats.Start();
            _http.Start();

            //ServerListReport = new ServerListReport(bind, 27900);
            //ServerListRetrieve = new ServerListRetrieve(bind, 28910);
            //LoginMasterServer = new LoginServerRetranslator(bind, 29900, 29901);
            //ChatServer = new ChatServerRetranslator(bind, 6667);
        }

        void OnError(Exception exception, bool send)
        {
            File.WriteAllText("LastClientError.ex", send + " " + exception.ToString());
            Logger.Error(exception);

            if (Debugger.IsAttached)
                Debugger.Break();

            Restart();
        }

        void Restart()
        {
            Stop();
            Start();
        }

        void OnChatAccept(TcpPortHandler handler, TcpClient client, CancellationToken token)
        {
            _inChat = false;
            _chatEncoded = false;
        }

        void OnClientManager(TcpPortHandler handler, byte[] buffer, int count)
        {
            var messages = ToUtf8(buffer, count).Split(new string[] { @"\final\" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < messages.Length; i++)
                HandleClientManagerMessage(handler, messages[i]);
        }

        private void HandleClientManagerMessage(TcpPortHandler handler, string mes)
        {
            Logger.Trace("CLIENT " + mes);
            var pairs = ParseHelper.ParseMessage(mes, out string query);

            if (pairs == null || string.IsNullOrWhiteSpace(query))
                return;

            if (pairs.ContainsKey("name") && !pairs.ContainsKey("email"))
            {
                var parts = pairs["name"].Split('@');

                if (parts.Length > 2)
                {
                    pairs["name"] = parts[0];
                    pairs["email"] = parts[1] + "@" + parts[2];
                }
            }

            switch (query)
            {
                case "login":
                    HandleLogin(pairs);
                    break;
                case "logout":
                    CoreContext.MasterServer.RequestLogout();
                    //Users.TryRemove(state.ProfileId, out LoginSocketState removingState);
                    //LoginServerMessages.Logout(ref state, keyValues);
                    break;
                case "registernick":
                    handler.Send(DataFunctions.StringToBytes(string.Format(@"\rn\{0}\id\{1}\final\", pairs["uniquenick"], pairs["id"])));
                    break;
                case "ka":
                    handler.SendAskii($@"\ka\\final\");
                    break;
                case "status":
                    HandleStatus(pairs);
                    break;
                default:
                    break;
            }
        }

        void HandleStatus(Dictionary<string, string> pairs)
        {
            _clientManager.SendAskii($@"\bdy\{0}\list\\final\");
        }

        void HandleLogin(Dictionary<string, string> pairs)
        {
            if (pairs.ContainsKey("uniquenick"))
                _name = pairs["uniquenick"];
            else
            {
                var parts = pairs["user"].Split('@');
                _name = parts[0];
                _email = parts[1] + "@" + parts[2];
            }

            _clientChallenge = pairs.GetOrDefault("challenge");
            _response = pairs.GetOrDefault("response");

            CoreContext.MasterServer.RequestLogin(_name);
        }

        void SendLoginResponce(LoginInfo loginInfo)
        {
            _email = loginInfo.Email;
            _profileId = loginInfo.ProfileId;
            _clientManager.Send(DataFunctions.StringToBytes(LoginHelper.BuildProofOrErrorString(loginInfo, _response, _clientChallenge, _serverChallenge)));
        }

        void OnSearchManager(TcpPortHandler handler, byte[] buffer, int count)
        {
            var str = ToUtf8(buffer, count);

            Debugger.Break();
        }

        void OnHttp(TcpPortHandler handler, byte[] buffer, int count)
        {
            var str = ToUtf8(buffer, count);

            Logger.Trace("HTTP " + str);

            HttpRequest request;

            using (var ms = new MemoryStream(buffer, 0, count, false, true))
                request = HttpHelper.GetRequest(ms);

            using (var ms = new MemoryStream())
            {
                if (request.Url.EndsWith("news.txt", StringComparison.OrdinalIgnoreCase))
                {
                    if (request.Url.EndsWith("Russiandow_news.txt", StringComparison.OrdinalIgnoreCase))
                        HttpHelper.WriteResponse(ms, HttpResponceBuilder.Text(RusNews, Encoding.Unicode));
                    else
                        HttpHelper.WriteResponse(ms, HttpResponceBuilder.Text(EnNews, Encoding.Unicode));
                    goto END;
                }

                if (request.Url.StartsWith("/motd/motd", StringComparison.OrdinalIgnoreCase))
                {
                    HttpHelper.WriteResponse(ms, HttpResponceBuilder.Text(RusNews, Encoding.Unicode));
                    goto END;
                }

                if (request.Url.StartsWith("/motd/vercheck", StringComparison.OrdinalIgnoreCase))
                {
                    //HttpHelper.WriteResponse(ms, HttpResponceBuilder.Text(@"\newver\1\newvername\1.4\dlurl\http://127.0.0.1/NewPatchHere.exe"));
                    HttpHelper.WriteResponse(ms, HttpResponceBuilder.Text(@"\newver\0", Encoding.UTF8));
                    goto END;
                }

                if (request.Url.EndsWith("LobbyRooms.lua", StringComparison.OrdinalIgnoreCase))
                {
                    HttpHelper.WriteResponse(ms, HttpResponceBuilder.Text(RoomPairs, Encoding.ASCII));
                    goto END;
                }

                if (request.Url.EndsWith("AutomatchDefaultsSS.lua", StringComparison.OrdinalIgnoreCase) || request.Url.EndsWith("AutomatchDefaultsDXP2Fixed.lua", StringComparison.OrdinalIgnoreCase))
                {
                    HttpHelper.WriteResponse(ms, HttpResponceBuilder.Text(AutomatchDefaults, Encoding.ASCII));
                    goto END;
                }

                /*if (request.Url.EndsWith("homepage.php.htm", StringComparison.OrdinalIgnoreCase))
                {
                    if (StatsResponce == null || (DateTime.Now - _lastStatsUpdate).TotalMinutes > 5)
                        StatsResponce = BuildTop10StatsResponce();

                    HttpHelper.WriteResponse(ms, StatsResponce);
                    goto END;
                }*/

                HttpHelper.WriteResponse(ms, HttpResponceBuilder.NotFound());

            END:
                handler.Send(ms.ToArray());
                handler.KillCurrentClient();
            }
        }

        void OnStats(TcpPortHandler handler, byte[] buffer, int count)
        {
            var str = ToUtf8(buffer, count);
            Logger.Trace("STATS " + str);
        }

        unsafe void OnChat(TcpPortHandler handler, byte[] buffer, int count)
        {
            if (_chatEncoded)
            {
                byte* bytesPtr = stackalloc byte[count];

                for (int i = 0; i < count; i++)
                    bytesPtr[i] = buffer[i];

                ChatCrypt.GSEncodeDecode(_chatClientKey, bytesPtr, count);

                for (int i = 0; i < count; i++)
                    buffer[i] = bytesPtr[i];
            }

            var str = ToUtf8(buffer, count);
            var lines = str.Split(ChatSplitChars, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
                HandleChatLine(handler, lines[i]);

        }

        unsafe void HandleChatLine(TcpPortHandler handler, string line)
        {
            Logger.Trace("CHATLINE "+line);

            var values = line.Split(ChatCommandsSplitChars, StringSplitOptions.RemoveEmptyEntries);

            if (line.StartsWith("LOGIN", StringComparison.OrdinalIgnoreCase)) { HandleLoginCommand(handler, values); return; }
            if (line.StartsWith("USRIP", StringComparison.OrdinalIgnoreCase)) { HandleUsripCommand(handler, values); return; }
            if (line.StartsWith("CRYPT", StringComparison.OrdinalIgnoreCase)) { HandleCryptCommand(handler, values); return; }
            if (line.StartsWith("USER", StringComparison.OrdinalIgnoreCase)) { HandleUserCommand(handler, values); return; }
            if (line.StartsWith("NICK", StringComparison.OrdinalIgnoreCase)) { HandleNickCommand(handler, values); return; }
            if (line.StartsWith("CDKEY", StringComparison.OrdinalIgnoreCase)) { HandleCdkeyCommand(handler, values); return; }
            if (line.StartsWith("JOIN", StringComparison.OrdinalIgnoreCase)) { HandleJoinCommand(handler, values); return; }
            if (line.StartsWith("MODE", StringComparison.OrdinalIgnoreCase)) { HandleModeCommand(handler, values); return; }
            if (line.StartsWith("QUIT", StringComparison.OrdinalIgnoreCase)) { HandleQuitCommand(handler, values); return; }
            if (line.StartsWith("GETCKEY", StringComparison.OrdinalIgnoreCase)) { HandleGetckeyCommand(handler, values); return; }
            if (line.StartsWith("PRIVMSG", StringComparison.OrdinalIgnoreCase)) { HandlePrivmsgCommand(handler, values); return; }

            Debugger.Break();

            //if (!state.Disposing && state.UserInfo != null)
            //   IrcDaemon.ProcessSocketMessage(state.UserInfo, asciValue);
        }

        void HandlePrivmsgCommand(TcpPortHandler handler, string[] values)
        {
            // PRIVMSG #GPG!1 :dfg
            var channelName = values[1];

            if (channelName == "#GPG!1")
            {
                var message = values[2].Substring(1);
                CoreContext.MasterServer.SendChatMessage(message);
            }
        }

        void HandleGetckeyCommand(TcpPortHandler handler, string[] values)
        {
            var channelName = values[1];

            var key = values[5];
            var id = values[3];

            if (key == ":\\username\\b_flags")
            {
                //GETCKEY #GPG!1 * 000 0 :\\username\\b_flags
                var users = CoreContext.MasterServer.GetAllUsers();

                for (int i = 0; i < users.Length; i++)
                {
                    var user = users[i];
                    // :s 702 elamaunt #GPG!1 pitbulwewe 050 :\XW9asqsfsX|8\sh
                    SendToClientChat($":s 702 {_name} {channelName} {user.UIName} {id} :\\{user.ClientUserName}\\{user.ClientBFlags}\r\n");
                }
            }


        }

        void HandleQuitCommand(TcpPortHandler handler, string[] values)
        {
            Restart();
        }

        void HandleModeCommand(TcpPortHandler handler, string[] values)
        {
            var channelName = values[1];

            if (channelName.StartsWith("#GPG", StringComparison.OrdinalIgnoreCase))
            {
                SendToClientChat($":s 324 {_name} {channelName} +\r\n");
            }
            else
            {

            }
        }

        void HandleJoinCommand(TcpPortHandler handler, string[] values)
        {
            var channelName = values[1];

            if (channelName.StartsWith("#GPG", StringComparison.OrdinalIgnoreCase))
            {
                var users = CoreContext.MasterServer.GetAllUsers();

                SendToClientChat($":{_user} JOIN {channelName}\r\n");
                SendToClientChat($":s 331 {channelName} :No topic is set\r\n");

                _inChat = true;

                var playersList = new StringBuilder();

                for (int i = 0; i < users.Length; i++)
                    playersList.Append(users[i].UIName+" ");

                SendToClientChat($":s 353 {_name} = {channelName} :@{playersList}\r\n");
                SendToClientChat($":s 366 {_name} {channelName} :End of NAMES list\r\n");
            }
            else
            {

            }
        }

        void HandleCdkeyCommand(TcpPortHandler handler, string[] values)
        {
            SendToClientChat($":s 706 {_name}: 1 :\"Authenticated\"\r\n");
        }

        void HandleUserCommand(TcpPortHandler handler, string[] values)
        {
            _user = $@"{_name}!{values[1]}@{handler.RemoteEndPoint.Address}";
        }

        void HandleNickCommand(TcpPortHandler handler, string[] values)
        {
            var users = CoreContext.MasterServer.GetAllUsers().Length;

            SendToClientChat($":SERVER!SERVER@* NOTICE {_name} :Authenticated\r\n");
            SendToClientChat($":s 001 {_name} :Welcome to the Matrix {_name}\r\n");
            SendToClientChat($":s 002 {_name} :Your host is xs0, running version 1.0\r\n");
            SendToClientChat($":s 003 {_name} :This server was created Fri Oct 19 1979 at 21:50:00 PDT\r\n");
            SendToClientChat($":s 004 {_name} s 1.0 iq biklmnopqustvhe\r\n");
            SendToClientChat($":s 375 {_name} :- (M) Message of the day - \r\n");
            SendToClientChat($":s 372 {_name} :- Welcome to GameSpy\r\n");
            SendToClientChat($":s 251 :There are {users} users and 0 services on 1 servers\r\n");
            SendToClientChat($":s 252 0 :operator(s)online\r\n");
            SendToClientChat($":s 253 {users} :unknown connection(s)\r\n");
            SendToClientChat($":s 254 1 :channels formed\r\n");
            SendToClientChat($":s 255 :I have {users} clients and 1 servers\r\n");
            SendToClientChat($":{_user} NICK {_name}\r\n");
        }

        unsafe void HandleCryptCommand(TcpPortHandler handler, string[] values)
        {
            _chatEncoded = true;
            _gamekeyBytes = null;

            if (values.Contains("whammer40kdc"))
            {
                _gameNameBytes = "whammer40kdc".ToAssciiBytes();
                _gamekeyBytes = "Ue9v3H".ToAssciiBytes();
            }

            if (values.Contains("whamdowfr"))
            {
                _gameNameBytes = "whamdowfr".ToAssciiBytes();
                _gamekeyBytes = "pXL838".ToAssciiBytes();
            }

            if (_gamekeyBytes == null)
            {
                Restart();
                return;
            }

            var chall = "0000000000000000".ToAssciiBytes();

            var clientKey = new ChatCrypt.GDCryptKey();
            var serverKey = new ChatCrypt.GDCryptKey();

            fixed (byte* challPtr = chall)
            {
                fixed (byte* gamekeyPtr = _gamekeyBytes)
                {
                    ChatCrypt.GSCryptKeyInit(clientKey, challPtr, gamekeyPtr, _gamekeyBytes.Length);
                    ChatCrypt.GSCryptKeyInit(serverKey, challPtr, gamekeyPtr, _gamekeyBytes.Length);
                }
            }

            _chatClientKey = clientKey;
            _chatServerKey = serverKey;

            handler.Send(DataFunctions.StringToBytes(":s 705 * 0000000000000000 0000000000000000\r\n"));
        }

        void HandleUsripCommand(TcpPortHandler handler, string[] values)
        {
            SendToClientChat($":s 302  :=+@{handler.RemoteEndPoint.Address}\r\n");
        }

        void HandleLoginCommand(TcpPortHandler handler, string[] values)
        {
            var nick = values[2];
            var loginInfo = CoreContext.MasterServer.GetLoginInfo(nick);

            SendToClientChat($":s 707 {nick} 12345678 {loginInfo.ProfileId}\r\n");

            RestartTimer();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        void RestartTimer()
        {
            StopTimer();
            _keepAliveTimer = new Timer(KeepAliveCallback, null, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        void StopTimer()
        {
            _keepAliveTimer?.Dispose();
            _keepAliveTimer = null;
        }

        void KeepAliveCallback(object state)
        {
            _heartbeatState++;

            Logger.Trace("sending keep alive");
            if (!_clientManager.Send(DataFunctions.StringToBytes(@"\ka\\final\")))
            {
                Restart();
                return;
            }

            // every 2nd keep alive request, we send an additional heartbeat
            if (_heartbeatState % 2 == 0)
            {
                Logger.Trace("sending heartbeat");
                if (!_clientManager.Send(DataFunctions.StringToBytes(String.Format(@"\lt\{0}\final\", RandomHelper.GetString(22, "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ][") + "__"))))
                {
                    Restart();
                    return;
                }
            }
        }

        unsafe void SendToClientChat(string message)
        {
            Logger.Trace("CHATSEND " + message);

            var bytesToSend = message.ToUTF8Bytes();

            if (_chatEncoded)
                fixed (byte* bytesToSendPtr = bytesToSend)
                    ChatCrypt.GSEncodeDecode(_chatServerKey, bytesToSendPtr, bytesToSend.Length);

            _chat.Send(bytesToSend);
        }

        void OnServerRetrieve(TcpPortHandler handler, byte[] buffer, int count)
        {
            var str = ToASCII(buffer, count);
            Logger.Trace("RETRIEVE " + str);

            string[] data = str.Split(new char[] { '\x00' }, StringSplitOptions.RemoveEmptyEntries);

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
                    SendRooms(handler, validate);
                    return;
                }
            }

            /*LoadLobbies()
               .ContinueWith(task =>
               {
                   if (task.Status != TaskStatus.RanToCompletion)
                   {
                       Console.WriteLine(task.Exception);
                       return;
                   }

                   GameServer[] servers;

                   var currentRating = ServerContext.ChatServer.CurrentRating;

                   if (isAutomatch)
                   {
                       // var groups = task.Result.GroupBy(s => s.GetByName("maxplayers") ?? string.Empty);
                       //servers = groups.SelectMany(x => x.OrderBy(s => Math.Abs(currentRating - GetServerRating(s))).Take(2)).ToArray();
                       servers = task.Result.OrderBy(s => Math.Abs(currentRating - GetServerRating(s))).ToArray();
                   }
                   else
                       servers = task.Result;

                   for (int i = 0; i < servers.Length; i++)
                   {
                       servers[i]["score_"] = ServerContext.ChatServer.CurrentRating.ToString();
                   }

                   string[] fields = data[5].Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

                   byte[] unencryptedServerList = PackServerList(state, servers, fields, isAutomatch);
                   byte[] encryptedServerList = GSEncoding.Encode("pXL838".ToAssciiBytes(), DataFunctions.StringToBytes(validate), unencryptedServerList, unencryptedServerList.LongLength);

                   SendToClient(state, encryptedServerList);
               });*/
        }

        void SendRooms(TcpPortHandler handler, string validate)
        {
            var bytes = new List<byte>();

            var remoteEndPoint = handler.RemoteEndPoint;
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

            //for (int i = 1; i <= 10; i++)
            //{
            bytes.Add(81);

            var b2 = BitConverter.GetBytes((long)1);

            bytes.Add(b2[3]);
            bytes.Add(b2[2]);
            bytes.Add(b2[1]);
            bytes.Add(b2[0]);

            bytes.Add(0);
            bytes.Add(0);

            bytes.Add(255);
            bytes.AddRange(DataFunctions.StringToBytes("Room 1"));
            bytes.Add(0);

            bytes.Add(255);
            bytes.AddRange(DataFunctions.StringToBytes(CoreContext.MasterServer.GetAllUsers().Length.ToString()));
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
            //}

            bytes.AddRange(new byte[] { 0, 255, 255, 255, 255 });

            var array = bytes.ToArray();

            byte[] enc = GSEncoding.Encode("pXL838".ToAssciiBytes(), DataFunctions.StringToBytes(validate), array, array.LongLength);

            handler.Send(enc);
        }

        void OnServerReport(UdpPortHandler handler, UdpReceiveResult result)
        {
            var str = ToUtf8(result.Buffer, result.Buffer.Length);
            Logger.Trace("REPORT " + str);

            var receivedBytes = result.Buffer;
            var remote = result.RemoteEndPoint;

            if (receivedBytes[0] == (byte)MessageType.AVAILABLE)
            {
                handler.Send(new byte[] { 0xfe, 0xfd, 0x09, 0x00, 0x00, 0x00, 0x00 }, remote);
            }
            else if (receivedBytes.Length > 5 && receivedBytes[0] == (byte)MessageType.HEARTBEAT)
            {
                // this is where server details come in, it starts with 0x03, it happens every 60 seconds or so

                var server = ParseHelper.ParseDetails(receivedBytes.Skip(5).ToArray());

               /* if (!CanRegisterServer(remote, receivedBytes.Skip(5).ToArray()))
                {
                    byte[] uniqueId = new byte[4];
                    Array.Copy(receivedBytes, 1, uniqueId, 0, 4);

                    byte[] response = new byte[] { 0xfe, 0xfd, (byte)MessageType.CHALLENGE_RESPONSE, uniqueId[0], uniqueId[1], uniqueId[2], uniqueId[3], 0x41, 0x43, 0x4E, 0x2B, 0x78, 0x38, 0x44, 0x6D, 0x57, 0x49, 0x76, 0x6D, 0x64, 0x5A, 0x41, 0x51, 0x45, 0x37, 0x68, 0x41, 0x00 };

                    handler.Send(response, remote);
                }*/
            }
            else if (receivedBytes.Length > 5 && receivedBytes[0] == (byte)MessageType.CHALLENGE_RESPONSE)
            {
                byte[] uniqueId = new byte[4];
                Array.Copy(receivedBytes, 1, uniqueId, 0, 4);

                byte[] validate = Encoding.UTF8.GetBytes("Iare43/78WkOVaU1Aanv8vrXbSwA\0");
                byte[] validateDC = Encoding.UTF8.GetBytes("Egn4q1jDYyOIVczkXvlGbBxavC4A\0");

                byte[] clientResponse = new byte[validate.Length];
                Array.Copy(receivedBytes, 5, clientResponse, 0, clientResponse.Length);

                var resStr = Encoding.UTF8.GetString(clientResponse);

                // if we validate, reply back a good response
                if (clientResponse.SequenceEqual(validate) || clientResponse.SequenceEqual(validateDC))
                {
                    byte[] response = new byte[] { 0xfe, 0xfd, 0x0a, uniqueId[0], uniqueId[1], uniqueId[2], uniqueId[3] };

                    handler.Send(response, remote);

                    AddValidServer(remote);
                }
            }
            else if (receivedBytes.Length == 5 && receivedBytes[0] == (byte)MessageType.KEEPALIVE)
            {
                // this is a server ping, it starts with 0x08, it happens every 20 seconds or so

                byte[] uniqueId = new byte[4];
                Array.Copy(receivedBytes, 1, uniqueId, 0, 4);
                RefreshServerPing(remote);
            }
        }

        void RefreshServerPing(IPEndPoint remote)
        {

        }

        void AddValidServer(IPEndPoint remote)
        {

        }

        bool CanRegisterServer(IPEndPoint remote, byte[] data)
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Stop()
        {
            _serverReport.Stop();
            _serverRetrieve.Stop();

            _clientManager.Stop();
            _searchManager.Stop();

            _chat.Stop();
            _stats.Stop();
            _http.Stop();
            StopTimer();
        }

        string ToUtf8(byte[] buffer, int count)
        {
            return Encoding.UTF8.GetString(buffer, 0, count);
        }

        string ToASCII(byte[] buffer, int count)
        {
            return Encoding.ASCII.GetString(buffer, 0, count);
        }

        string RusNews => @" Привет! Вы на сервере elamaunt'а под названием THUNDERHAWK
.
Добро пожаловать на бетатест сервера.
Функционально сервер почти полностью готов. Остались некоторые незначительные ошибки и невозможность играть командой в автоматче.
Статистика начисляется только на последней версии мода ThunderHawk (обновляется автоматически при запуске лаунчера).
Bugfix 1.56a и фикс пафинга (для Техники) внедрены в этот мод. В будущем будут новые изменения для благоприятной игры.
.
Текущие карты в авто: 
.
1на1
- [TP MOD]edemus gamble
- 2P Shrine of Excellion
- 2P Meeting of Minds
- 2P Battle Marshes
- 2P Outer Reaches (доработанная Дэвилом)
- 2P Blood River
- 2P Fata Morgana
- 2P Titan Fall (доработанная Дэвилом)
- 2P Tranquilitys End (доработанная  Дэвилом)
- 2P FraziersDemise (доработанная Дэвилом)
- 2P Moonbase (доработанная Дэвилом)
.
2на2
- 4p gurmuns pass
- 4P Biffys Peril
- 4P Gorhael Crater
- 4P Doom Spiral
- 4p panrea lowlands (доработанная Дэвилом)
- 4P Skerries (доработанная Дэвилом)
- 4p Saints Square 
.
3на3
- 6p Mortalis
- 6P Alvarus
- 6P Shakun Coast
- 6P Fury Island
- 6p paynes retribution
.
4на4
- 8P Oasis of Sharr
- 8P Forbidden Jungle
- 8P Jalaganda Lowlands
- 8p cerulea
- 8p burial grounds
- 8p thurabis plateau
.
.
Сервер активно обсуждается здесь и в Дискорд.
http://forums.warforge.ru/ (RUS)";

        string EnNews => @" Hello! You are on elamaunt's server THUNDERHAWK.
.
Wellcome on betatest.
The server is almost complete. There are some minor bugs and the inability to play as a team of friends in automatch.
Statistics changes only on the latest version of the ThunderHawk mod (automatically updates by launcher as startup).
Bugfix mod 1.56a and pathfinding (for vehicle) fix are introduced in this mod. In the future there will be new changes for an auspicious game.
.
Current maps in automatch: 
.
1vs1
- [TP MOD]edemus gamble
- 2P Shrine of Excellion
- 2P Meeting of Minds
- 2P Battle Marshes
- 2P Outer Reaches (fixed by Devil)
- 2P Blood River
- 2P Fata Morgana
- 2P Titan Fall (fixed by Devil)
- 2P Tranquilitys End (fixed by Devil)
- 2P FraziersDemise (fixed by Devil)
- 2P Moonbase (fixed by Devil)
.
2vs2
- 4p gurmuns pass
- 4P Biffys Peril
- 4P Gorhael Crater
- 4P Doom Spiral
- 4p panrea lowlands (fixed by Devil)
- 4P Skerries (fixed by Devil)
- 4p Saints Square 
.
3vs3
- 6p Mortalis
- 6P Alvarus
- 6P Shakun Coast
- 6P Fury Island
- 6p paynes retribution
.
4на4
- 8P Oasis of Sharr
- 8P Forbidden Jungle
- 8P Jalaganda Lowlands
- 8p cerulea
- 8p burial grounds
- 8p thurabis plateau
.
.
The server is being actively discussed here and on Discord.
http://forums.warforge.ru/ (RUS)";

        string RoomPairs => @"
room_pairs = 
{
        ""Room 1"",
		""ThunderHawk""
}
                    ";

        public string AutomatchDefaults => @"----------------------------------------------------------------------------------------------------------------
-- Default FE Settings
-- (c) 2004 Relic Entertainment Inc.

-- chat spam defined.
chat_options =
{
	maxChat = 30,
	timeInterval = 60,
	timeWait = 60,
}

-- Note: automatch defaults are defined in code for w40k
automatch_defaults =
{
	-- win conditions: IDs listed here
	win_condition_defaults = 
	{
		""Annihilate"",
		""ControlArea"",
		""StrategicObjective""
	},

	--automatch maps
	automatch_maps2p = 
	{
		""2p_Fallen_City"",
		""2P_Battle_Marshes"",
		""2P_Deadmans_Crossing"",
		""2P_Meeting_of_Minds"",
		""2P_Outer_Reaches"",
		""2P_Valley_of_Khorne"",
		""2P_Blood_River""
	},
	automatch_maps4p = 
	{
		""4P_Biffys_Peril"",
		""4P_Quatra"",
		""4P_Tartarus_Center"",
		""4P_volcanic reaction"",
		""4P_Mountain_Trail"",
		""4P_Tainted_Place"",
		""4P_Tainted_Soul"",
		""4p_Saints_Square""
	},
	automatch_maps6p = 
	{
		""6P_Bloodshed_Alley"",
		""6P_Kasyr_Lutien"",
		""6p_Mortalis"",
		""6P_Testing_Grounds"",
		""6PTeam_Streets_of_Vogen"",
		""6p_crossroads""
	},
	automatch_maps8p = 
	{
		""8p_team_ruins"",
		""8P_Burial_Grounds"",
		""8P_Daturias_Pits"",
		""8P_Doom_Chamber"",
		""8P_Lost_Hope"",
		""8P_Penal_Colony""		
	},
}

-- Note: automatch defaults are defined in code for wxp
automatch_defaults_wxp =
{
	-- win conditions: IDs listed here
	win_condition_defaults = 
	{
		""Annihilate"",
		""ControlArea"",
		""StrategicObjective""
	},

	--automatch maps
	automatch_maps2p = 
	{
		""2P_Battle_Marshes"",
		""2P_Meeting_of_Minds"",
		""2P_Outer_Reaches"",
		""2P_Blood_River"",
		""2p_Fallen_City"",
		""2P_Shrine_of_Excellion"",
		""4P_Gorhael_Crater"",
		""4P_Tiboraxx"",
		""4P_Dread_Peak"",
		""2P_Quests_Triumph"",
	},
	automatch_maps4p = 
	{
		""4P_Biffys_Peril"",
		""4P_Gorhael_Crater"",
		""4P_Tiboraxx"",
		""4P_Dread_Peak"",
		""4P_Torrents"",
		""4P_Ice_Flow"",
		""4P_Doom_Spiral"",
		
	},
	automatch_maps6p = 
	{
		""6p_Mortalis"",
		""6P_Crozius_Arcanum"",
		""6P_Fury_Island"",
		""6P_Thargorum"",
		""6P_Alvarus"",

	},
	automatch_maps8p = 
	{
		""8P_Oasis_of_Sharr"",
		""8P_Forbidden_Jungle"",
		""8P_Fear of the Darkness"",
	
	},
}

-- Note: automatch defaults are defined in code for dxp2
automatch_defaults_dxp2 =
{
	-- win conditions: IDs listed here
	win_condition_defaults = 
	{
		""Annihilate"",
		""ControlArea"",
		""StrategicObjective""
	},


	--automatch maps
	automatch_maps2p = 
	{
		""[TP MOD]edemus gamble"",
		""2P_Shrine_of_Excellion"",
		""2P_Meeting_of_Minds"",
		""2P_Battle_Marshes"",
		""2P_Outer_Reaches"",
		""2P_Blood_River"",
		""2P_Fata_Morgana"",
		""2P_Titan_Fall"",
		""2P_Tranquilitys_End"",
		""2P_FraziersDemise"",
		""2P_Moonbase""
	},
	automatch_maps4p = 
	{
		""4p_gurmuns_pass"",
		""4P_Biffys_Peril"",
		""4P_Gorhael_Crater"",
		""4P_Doom_Spiral"",
		""4p_panrea_lowlands"",
		""4P_Skerries"",
		""4p_Saints_Square""
	},
	automatch_maps6p = 
	{
		""6p_Mortalis"",
		""6P_Alvarus"",
		""6P_Shakun_Coast"",
		""6P_Fury_Island"",
		""6p_paynes_retribution""
	},
	automatch_maps8p = 
	{
		""8P_Oasis_of_Sharr"",
		""8P_Forbidden_Jungle"",
		""8P_Jalaganda_Lowlands"",
		""8p_cerulea"",
		""8p_burial_grounds"",
		""8p_thurabis_plateau""
	},
}


";
    }
}
