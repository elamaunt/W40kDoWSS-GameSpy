using Framework;
using GSMasterServer.Utils;
using Http;
using Reality.Net.Extensions;
using Reality.Net.GameSpy.Servers;
using SharedServices;
using Steamworks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThunderHawk.Core;
using ThunderHawk.StaticClasses.Soulstorm;
using ThunderHawk.Utils;
using static ThunderHawk.TcpPortHandler;

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
        string _shortUser;
        string _name;
        long _profileId;
        string _email;
        string _response;
        bool _inChat;
        bool _challengeResponded;

        string _enteredLobbyHash;
        string _localServerHash;
        GameServerDetails _localServer;
        string _flags;

        byte[] _gameNameBytes;
        byte[] _gamekeyBytes;

        ChatCrypt.GDCryptKey _chatClientKey;
        ChatCrypt.GDCryptKey _chatServerKey;

        readonly char[] ChatSplitChars = new[] { '\r', '\n' };
        const char PrefixCharacter = ':';

        const string XorKEY = "GameSpy3D";
        volatile int _sessionCounter;

        CancellationTokenSource _lobbyTokenSource;
        volatile bool _gameLaunchReceived;

        CancellationToken Token => _lobbyTokenSource.Token;

        readonly ConcurrentDictionary<string, GameServerDetails> _lastLoadedLobbies = new ConcurrentDictionary<string, GameServerDetails>();

        public event Action<GameHostInfo[]> LobbiesUpdatedByRequest;

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
            _serverRetrieve = new TcpPortHandler(28910, new RetrieveTcpSetting(), OnServerRetrieve, OnServerRetrieveError);
            
            _clientManager = new TcpPortHandler(29900, new LoginTcpSetting(), OnClientManager, OnError, OnClientAccept, OnZeroBytes);
            _searchManager = new TcpPortHandler(29901, new LoginTcpSetting(), OnSearchManager, OnError, null, OnZeroBytes);

            _chat = new TcpPortHandler(6667, new ChatTcpSetting(), OnChat, OnError, OnChatAccept, OnZeroBytes);
            _stats = new TcpPortHandler(29920, new StatsTcpSetting(), OnStats, OnError, OnStatsAccept, OnZeroBytes);
            _http = new TcpPortHandler(80, new HttpTcpSetting(), OnHttp, OnError, null, OnZeroBytes);

            
            CoreContext.MasterServer.NewUserReceived += OnNewUserReceived;
            CoreContext.MasterServer.UserNameChanged += OnUserNameChanged;
            CoreContext.MasterServer.UserKeyValueChanged += OnUserKeyValueChanged;
            CoreContext.MasterServer.UserConnected += OnUserConnected;
            CoreContext.MasterServer.UserDisconnected += OnUserDisconnected;
            CoreContext.MasterServer.LoginInfoReceived += SendLoginResponce;
            CoreContext.MasterServer.ChatMessageReceived += OnChatMessageReceived;
            CoreContext.MasterServer.ConnectionLost += OnServerConnectonLost;
            CoreContext.MasterServer.Connected += OnServerConnected;
            CoreContext.MasterServer.NicksReceived += OnNicksReceived;
            CoreContext.MasterServer.NameCheckReceived += OnNameCheckReceived;
            CoreContext.MasterServer.LoginErrorReceived += OnLoginErrorReceived;

            SteamLobbyManager.LobbyChatMessage += OnLobbyChatMessageReceived;
            //SteamLobbyManager.LobbyMemberUpdated += OnLobbyMemberUpdated;
            SteamLobbyManager.LobbyMemberLeft += OnLobbyMemberLeft;
            SteamLobbyManager.TopicUpdated += OnTopicUpdated;
        }

        void OnNewUserReceived(string name, long? id, string email)
        {
            if (string.IsNullOrWhiteSpace(name) || id == null)
            {
                _clientManager.SendAskii(@"\error\\err\516\fatal\\errmsg\This account name is already in use!\id\1\final\");
            }
            else
            {
                _clientManager.SendAskii(string.Format(@"\nur\\userid\{0}\profileid\{1}\id\1\final\", id + 10000000, id));
            }
        }

        void OnServerRetrieveError(Exception exception, bool send, int port)
        {
            Logger.Info(exception);
        }

        void OnUserKeyValueChanged(string name, string key, string value)
        {
            if (name == null || _name == name)
                return;

            SendToClientChat($":s 702 #GPG!1 #GPG!1 {name} BCAST :\\{key}\\{value}\r\n");
        }

        void OnLoginErrorReceived(string name)
        {
            _clientManager.Send(DataFunctions.StringToBytes(@"\error\\err\0\fatal\\errmsg\Invalid Query!\id\1\final\"));
        }

        void OnNameCheckReceived(string name, long? profileId)
        {
            if (profileId == null)
                _searchManager.Send(DataFunctions.StringToBytes(String.Format(@"\error\\err\265\fatal\\errmsg\Username [{0}] doesn't exist!\id\1\final\", name)));
            else
                _searchManager.Send(DataFunctions.StringToBytes($@"\cur\0\pid\{profileId}\final\"));
        }

        void OnNicksReceived(string[] nicks)
        {
            if (nicks.IsNullOrEmpty())
            {
                _searchManager.Send(DataFunctions.StringToBytes(@"\nr\0\ndone\\final\"));
                //_searchManager.Send(DataFunctions.StringToBytes(@"\error\\err\551\fatal\\errmsg\Unable to get any associated profiles.\id\1\final\"));
                return;
            }

            if (nicks.Length == 0)
            {
                _searchManager.Send(DataFunctions.StringToBytes(@"\nr\0\ndone\\final\"));
                return;
            }

            _searchManager.Send(DataFunctions.StringToBytes(GenerateNicks(nicks)));
        }

        void OnUserNameChanged(UserInfo user, long? previousProfile, string previousName, string newName)
        {
            if (user.SteamId == SteamUser.GetSteamID().m_SteamID)
                return;

            if (previousName != null && previousProfile.HasValue)
                SendToClientChat($":{previousName}!X{GetEncodedIp(user, previousName)}X|{previousProfile.Value}@127.0.0.1 PART #GPG!1 :Leaving\r\n");

            if (!user.IsProfileActive)
                return;

            SendToClientChat($":{newName}!X{GetEncodedIp(user, newName)}X|{user.ActiveProfileId}@127.0.0.1 JOIN #GPG!1\r\n");

            if (user.BStats != null)
                SendToClientChat($":s 702 #GPG!1 #GPG!1 {newName} BCAST :\\b_stats\\{user.BStats}\r\n");
            else
                SendToClientChat($":s 702 #GPG!1 #GPG!1 {newName} BCAST :\\b_stats\\{user.ActiveProfileId ?? 0}|{user.Score1v1 ?? 1000}|{user.StarsCount}|\r\n");

            if (user.BFlags != null)
                SendToClientChat($":s 702 #GPG!1 #GPG!1 {newName} BCAST :\\b_flags\\{user.BFlags}\r\n");
        }

        void OnUserConnected(UserInfo user)
        {
            if (user.SteamId == SteamUser.GetSteamID().m_SteamID)
                return;

            if (user.IsProfileActive)
            {
                SendToClientChat($":{user.Name}!X{GetEncodedIp(user, user.Name)}X|{user.ActiveProfileId}@127.0.0.1 JOIN #GPG!1\r\n");

                if (user.BStats != null)
                    SendToClientChat($":s 702 #GPG!1 #GPG!1 {user.Name} BCAST :\\b_stats\\{user.BStats}\r\n");
                else
                    SendToClientChat($":s 702 #GPG!1 #GPG!1 {user.Name} BCAST :\\b_stats\\{user.ActiveProfileId ?? 0}|{user.Score1v1 ?? 1000}|{user.StarsCount}|\r\n");

                if (user.BFlags != null)
                    SendToClientChat($":s 702 #GPG!1 #GPG!1 {user.Name} BCAST :\\b_flags\\{user.BFlags}\r\n");
            }
        }

        void OnUserDisconnected(UserInfo user)
        {
            if (user.SteamId == SteamUser.GetSteamID().m_SteamID)
                return;

            if (user.IsProfileActive)
                SendToClientChat($":{user.UIName}!X{GetEncodedIp(user, user.UIName)}X|{user.ActiveProfileId}@127.0.0.1 PART #GPG!1 :Leaving\r\n");
        }

        void OnStatsAccept(TcpPortHandler handler, TcpClientNode node, CancellationToken token)
        {
            handler.Send(XorBytes(@"\lc\1\challenge\KNDVKXFQWP\id\1\final\", XorKEY, 7));
        }

        void OnServerConnected()
        {
            if (CoreContext.LaunchService.GameProcess != null)
                Start();
        }

        void OnServerConnectonLost()
        {
            Stop();
        }

        void OnLobbyChatMessageReceived(ulong memberId, string message)
        {
            Logger.Info("RECV-FROM-LOBBY-CHAT " + message);

            var values = GetLineValues(message);

            if (message.StartsWith("UTM", StringComparison.OrdinalIgnoreCase)) { HandleRemoteUtmCommand(values); return; }
            if (message.StartsWith("PRIVMSG", StringComparison.OrdinalIgnoreCase)) { HandleRemotePrivmsgCommand(values); return; }
            
            if (memberId == SteamUser.GetSteamID().m_SteamID)
                return;

            if (message.StartsWith("JOIN", StringComparison.OrdinalIgnoreCase)) { HandleRemoteJoinCommand(values); return; }
            if (message.StartsWith("SETCKEY", StringComparison.OrdinalIgnoreCase)) { HandleRemoteSetckeyCommand(values); return; }

            Debugger.Break();
        }

        void HandleRemoteJoinCommand(string[] values)
        {
            // :Bambochuk2!Xu4FpqOa9X|4@192.168.159.128 JOIN #GSP!whamdowfr!76561198408785287

            if (values[1] == _user)
            {
                UpdateGameLaunchState();
                return;
            }

            var userValues = values[1].Split(new char[] { '!', '|', '@' });
            var nick = userValues[0];

            SendToClientChat($":{values[1]} JOIN #GSP!whamdowfr!{_enteredLobbyHash}\r\n");

            UpdateGameLaunchState();

            /*SendToClientChat($":{nick}!Xu4FpqOa9X|{userValues[2]}@192.168.159.128 JOIN #GSP!whamdowfr!{_enteredLobbyHash}\r\n");
            SendToClientChat($":s 702 #GPG!1 #GPG!1 {nick} BCAST :\\b_flags\\s\r\n");
            SendToClientChat($":s 702 #GSP!whamdowfr!{_enteredLobbyHash} #GSP!whamdowfr!{_enteredLobbyHash} {nick} BCAST :\\b_flags\\s\r\n");*/
        }

        void UpdateGameLaunchState()
        {
            _gameLaunchReceived = SteamLobbyManager.IsLobbyFull;

            if (_gameLaunchReceived)
            {
                Logger.Info("LOBBY IF FULL NOW");
            }
        }

        void HandleRemoteSetckeyCommand(string[] values)
        {
            var channelName = values[1];

            if (channelName.StartsWith("#GSP", StringComparison.OrdinalIgnoreCase))
            {
                var keyValues = values[3];

                var pairs = keyValues.Split(':', '\\');

                for (int i = 1; i < pairs.Length; i += 2)
                    SendToClientChat($":s 702 #GSP!whamdowfr!{_enteredLobbyHash} #GSP!whamdowfr!{_enteredLobbyHash} {values[2]} BCAST :\\{pairs[i]}\\{pairs[i + 1]}\r\n");
            }
        }

        void HandleRemotePrivmsgCommand(string[] values)
        {
            if (!SteamLobbyManager.IsInLobbyNow)
                return;

            SendToClientChat($":{_user} PRIVMSG #GSP!whamdowfr!{_enteredLobbyHash} :{values[2]}\r\n");
        }

        void HandleRemoteUtmCommand(string[] values)
        {
            //if (!SteamLobbyManager.IsInLobbyNow)
            //    return;

            SendToClientChat($":{_user} UTM #GSP!whamdowfr!{_enteredLobbyHash} :{values[2]}\r\n");
        }

        void OnTopicUpdated(string topic)
        {
            if (!SteamLobbyManager.IsInLobbyNow)
                return;
        }

        void OnLobbyMemberLeft(ulong memberSteamId, bool disconnected)
        {
            var info = CoreContext.MasterServer.GetUserInfo(memberSteamId);

            if (info == null)
                return;

            SendToClientChat($":{info.Name}!X{GetEncodedIp(info, info.Name)}X|{info.ActiveProfileId}@127.0.0.1 PART #GSP!whamdowfr!{_enteredLobbyHash} :Leaving\r\n");
        }

        void OnChatMessageReceived(MessageInfo message)
        {
            if (!_inChat)
                return;

            if (message.FromGame && message.Author.SteamId == SteamUser.GetSteamID().m_SteamID)
                return;

            SendToClientChat($":{message.Author.UIName}!XaaaaaaaaX|1@127.0.0.1 PRIVMSG #GPG!1 :{message.Text}\r\n");
        }

        void OnZeroBytes(TcpPortHandler handler)
        {
            Restart();
        }

        void OnClientAccept(TcpPortHandler handler, TcpClientNode node, CancellationToken token)
        {
            //Обновляем челендж для нового соединения
            _serverChallenge = RandomHelper.GetString(10);
            handler.Send(node, DataFunctions.StringToBytes($@"\lc\1\challenge\{_serverChallenge}\id\1\final\"));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Start()
        {
            _serverReport.Start();
            _serverRetrieve.Start();

            _clientManager.Start();
            _searchManager.Start();

            _chat.Start();
            _stats.Start();
            _http.Start();

            //ServerListReport = new ServerListReport(bind, 27900);
            //ServerListRetrieve = new ServerListRetrieve(bind, 28910);
            //LoginMasterServer = new LoginServerRetranslator(bind, 29900, 29901);
            //ChatServer = new ChatServerRetranslator(bind, 6667);
        }

        void OnError(Exception exception, bool send, int port)
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
            RecreateLobbyToken();
            Start();
        }

        void OnChatAccept(TcpPortHandler handler, TcpClientNode node, CancellationToken token)
        {
            _inChat = false;
            _chatEncoded = false;
        }

        void OnClientManager(TcpPortHandler handler, TcpClientNode node, byte[] buffer, int count)
        {
            var messages = ToUtf8(buffer, count).Split(new string[] { @"\final\" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < messages.Length; i++)
                HandleClientManagerMessage(handler, node, messages[i]);
        }

        private void HandleClientManagerMessage(TcpPortHandler handler, TcpClientNode node, string mes)
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
                    HandleLogin(node, pairs);
                    RestartTimer(node);
                    break;
                case "logout":
                    CoreContext.MasterServer.RequestLogout();
                    SteamLobbyManager.LeaveFromCurrentLobby();
                    break;
                case "registernick":
                    handler.Send(node, DataFunctions.StringToBytes(string.Format(@"\rn\{0}\id\{1}\final\", pairs["uniquenick"], pairs["id"])));
                    break;
                case "ka":
                    handler.SendAskii(node, $@"\ka\\final\");
                    break;
                case "status":
                    HandleStatus(node, pairs);
                    break;
                case "newuser":
                    CoreContext.MasterServer.RequestNewUser(pairs);
                    break;
                case "getprofile":
                    // TODO
                    break;
                default:
                    Debugger.Break();
                    break;
            }
        }

        void HandleStatus(TcpClientNode node, Dictionary<string, string> pairs)
        {
            _clientManager.SendAskii(node, $@"\bdy\{0}\list\\final\");

            var status = pairs.GetOrDefault("status") ??  "0";
            var statusString = pairs.GetOrDefault("statstring") ?? "Offline";
            var locString = pairs.GetOrDefault("locstring") ?? "-1";
           
            string lsParameter;

            if (string.IsNullOrWhiteSpace(locString) || locString == "0")
                lsParameter = string.Empty;
            else
                lsParameter = "|ls|" + locString;

            var statusResult = $@"\bm\100\f\{_profileId}\msg\|s|{status}{lsParameter}|ss|{statusString}\final\";

            _clientManager.SendAskii(node, statusResult);
        }

        void HandleLogin(TcpClientNode node, Dictionary<string, string> pairs)
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

        void SendLoginResponce( LoginInfo loginInfo)
        {
            _email = loginInfo.Email;
            _profileId = loginInfo.ProfileId;

            /*if (CoreContext.LaunchService.GetCurrentModName() != "thunderhawk")
            {
                _clientManager.Send(DataFunctions.StringToBytes(@"\error\\err\0\fatal\\errmsg\You can login only with ThunderHawk active mod.\id\1\final\"));
                MessageBox.Show("You can login only with ThunderHawk active mod.");
            }
            else*/

            _heartbeatState = 0;
            _clientManager.Send(DataFunctions.StringToBytes(LoginHelper.BuildProofOrErrorString(loginInfo, _response, _clientChallenge, _serverChallenge)));
        }

        void OnSearchManager(TcpPortHandler handler, TcpClientNode node, byte[] buffer, int count)
        {
            var str = ToUtf8(buffer, count);
            var pairs = ParseHelper.ParseMessage(str, out string query);


            switch (query)
            {
                case "nicks":
                    {
                        // \\nicks\\\\email\\elamaunt3@gmail.com\\passenc\\J4PGhRi[\\namespaceid\\7\\partnerid\\0\\gamename\\whamdowfr\\final\\

                        if (!pairs.ContainsKey("email") || (!pairs.ContainsKey("passenc") && !pairs.ContainsKey("pass")))
                        {
                            handler.Send(node, DataFunctions.StringToBytes(@"\error\\err\0\fatal\\errmsg\Invalid Query!\id\1\final\"));
                            return;
                        }

                        string password = String.Empty;
                        if (pairs.ContainsKey("passenc"))
                        {
                            password = DecryptPassword(pairs["passenc"]);
                        }
                        else if (pairs.ContainsKey("pass"))
                        {
                            password = pairs["pass"];
                        }

                        password = password.ToMD5();

                        CoreContext.MasterServer.RequestAllUserNicks(pairs["email"]);
                        return;
                    }
                case "check":
                    {
                        string name = String.Empty;

                        if (String.IsNullOrWhiteSpace(name))
                        {
                            if (pairs.ContainsKey("uniquenick"))
                            {
                                name = pairs["uniquenick"];
                            }
                        }
                        if (String.IsNullOrWhiteSpace(name))
                        {
                            if (pairs.ContainsKey("nick"))
                            {
                                name = pairs["nick"];
                            }
                        }

                        if (String.IsNullOrWhiteSpace(name))
                        {
                            handler.Send(node, DataFunctions.StringToBytes(@"\error\\err\0\fatal\\errmsg\Invalid Query!\id\1\final\"));
                            return;
                        }

                        CoreContext.MasterServer.RequestNameCheck(name);
                        return;
                    }
                default:
                    break;
            }

            Debugger.Break();
        }

        string GenerateNicks(string[] nicks)
        {
            string message = @"\nr\" + nicks.Length;

            for (int i = 0; i < nicks.Length; i++)
                message += String.Format(@"\nick\{0}\uniquenick\{0}", nicks[i]);

            message += @"\ndone\final\";
            return message;
        }

        void OnHttp(TcpPortHandler handler, TcpClientNode node, byte[] buffer, int count)
        {
            try
            {
                var str = ToUtf8(buffer, count); 

                Logger.Trace("HTTP CLIENT HASH " + node.GetHashCode());
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
                        //HttpHelper.WriteResponse(ms, HttpResponceBuilder.TextFileBytes(CoreContext.MasterServer.AutomatchDefaultsBytes));
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
                    Logger.Trace("HTTP WANT TO SEND " + node.GetHashCode()+" "+ ms.Length);
                    handler.Send(node, ms.ToArray());
                    handler.KillClient(node);
                }
            }
            catch(InvalidDataException ex)
            {
                Logger.Info(ex);
            }
        }

        void OnStats(TcpPortHandler handler, TcpClientNode node, byte[] buffer, int count)
        {
            var str = Encoding.UTF8.GetString(XorBytes(buffer, 0, count - 7, XorKEY), 0, count);

            Logger.Trace("STATS " + str);

            if (str.StartsWith(@"\auth\\gamename\", StringComparison.OrdinalIgnoreCase))
            {
                var sesskey = Interlocked.Increment(ref _sessionCounter).ToString("0000000000");

                handler.Send(node, XorBytes($@"\lc\2\sesskey\{sesskey}\proof\0\id\1\final\", XorKEY, 7));
                return;
            }

            if (str.StartsWith(@"\authp\\pid\", StringComparison.OrdinalIgnoreCase))
            {
                var pid = GetPidFromInput(str, 12);
                var profileId = long.Parse(pid);

                handler.Send(node, XorBytes($@"\pauthr\{pid}\lid\1\final\", XorKEY, 7));
                return;
            }

            if (str.StartsWith(@"\getpd\", StringComparison.OrdinalIgnoreCase))
            {
                // \\getpd\\\\pid\\87654321\\ptype\\3\\dindex\\0\\keys\\\u0001points\u0001points2\u0001points3\u0001stars\u0001games\u0001wins\u0001disconn\u0001a_durat\u0001m_streak\u0001f_race\u0001SM_wins\u0001Chaos_wins\u0001Ork_wins\u0001Tau_wins\u0001SoB_wins\u0001DE_wins\u0001Eldar_wins\u0001IG_wins\u0001Necron_wins\u0001lsw\u0001rnkd_vics\u0001con_rnkd_vics\u0001team_vics\u0001mdls1\u0001mdls2\u0001rg\u0001pw\\lid\\1\\final\\
                // \getpd\\pid\87654321\ptype\3\dindex\0\keys\pointspoints2points3starsgameswinsdisconna_duratm_streakf_raceSM_winsChaos_winsOrk_winsTau_winsSoB_winsDE_winsEldar_winsIG_winsNecron_winslswrnkd_vicscon_rnkd_vicsteam_vicsmdls1mdls2rgpw\lid\1\final\
                var pid = GetPidFromInput(str, 12);

                var keysIndex = str.IndexOf("keys") + 5;
                var keys = str.Substring(keysIndex);
                var keysList = keys.Split(new string[] { "\u0001", "\\lid\\1\\final\\", "final", "\\", "lid" }, StringSplitOptions.RemoveEmptyEntries);

                var keysResult = new StringBuilder();
                var stats = CoreContext.MasterServer.GetStatsInfo(long.Parse(pid)); //ProfilesCache.GetProfileByPid(pid);

                for (int i = 0; i < keysList.Length; i++)
                {
                    var key = keysList[i];

                    keysResult.Append("\\" + key + "\\");

                    switch (key)
                    {
                        case "points": keysResult.Append(stats.Score1v1); break;
                        case "points2": keysResult.Append(stats.Score2v2); break;
                        case "points3": keysResult.Append(stats.Score3v3_4v4); break;
                        case "stars": keysResult.Append(stats.StarsCount); break;
                        case "games": keysResult.Append(stats.GamesCount); break;
                        case "wins": keysResult.Append(stats.WinsCount); break;
                        case "disconn": keysResult.Append(stats.Disconnects); break;
                        case "a_durat": keysResult.Append(stats.AverageDuration); break;
                        case "m_streak": keysResult.Append(stats.Best1v1Winstreak); break;
                        case "f_race": keysResult.Append(stats.FavouriteRace); break;

                       /* case "SM_wins": keysResult.Append("0"); break;
                        case "Chaos_wins": keysResult.Append("0"); break;
                        case "Ork_wins": keysResult.Append("0"); break;
                        case "Tau_wins": keysResult.Append("0"); break;
                        case "SoB_wins": keysResult.Append("0"); break;
                        case "DE_wins": keysResult.Append("0"); break;
                        case "Eldar_wins": keysResult.Append("0"); break;
                        case "IG_wins": keysResult.Append("0"); break;
                        case "Necron_wins": keysResult.Append("0"); break;
                        case "lsw": keysResult.Append("0"); break;
                        case "rnkd_vics": keysResult.Append("0"); break;
                        case "con_rnkd_vics": keysResult.Append("0"); break;
                        case "team_vics": keysResult.Append("0"); break;
                        case "mdls1": keysResult.Append("0"); break;
                        case "mdls2": keysResult.Append("0"); break;
                        case "rg": keysResult.Append("0"); break;
                        case "pw": keysResult.Append("0"); break;*/
                        default:
                            keysResult.Append("0");
                            break;
                    }

                }

                handler.Send(node, XorBytes($@"\getpdr\1\lid\1\pid\{pid}\mod\{stats.Modified}\length\{keys.Length}\data\{keysResult}\final\", XorKEY, 7));

                return;
            }

            if (str.StartsWith(@"\setpd\", StringComparison.OrdinalIgnoreCase))
            {
                var pid = GetPidFromInput(str, 12);

                var lidIndex = str.IndexOf("\\lid\\", StringComparison.OrdinalIgnoreCase);
                var lid = str.Substring(lidIndex + 5, 1);

                var timeInSeconds = (ulong)((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
                // \setpd\\pid\3\ptype\1\dindex\0\kv\1\lid\1\length\413\data\\ckey\5604 - 7796 - 6425 - 0127 - DA96\system\Nr.Proc:8, Type: 586, GenuineIntel, unknown: f = 6,m = 12, Fam: 6, Mdl: 12, St: 3, Fe: 7, OS: 7, Ch: 15\speed\CPUSpeed: 3.5Mhz\os\OS NT 6.2\lang\Language: Русский(Россия), Country: Россия, User Language:Русский(Россия), User Country:Россия\vid\Card: Dx9: Hardware TnL, NVIDIA GeForce GTX 1080, \vidmod\Mode: 1920 x 1080 x 32\mem\2048Mb phys. memory
                handler.Send(node, XorBytes($@"\setpdr\1\lid\{lid}\pid\{pid}\mod\{timeInSeconds}\final\", XorKEY, 7));

                return;
            }

            if (str.StartsWith(@"\updgame\", StringComparison.OrdinalIgnoreCase))
            {
                var gamedataIndex = str.IndexOf("gamedata");
                var finalIndex = str.IndexOf("final");

                var gameDataString = str.Substring(gamedataIndex + 9, finalIndex - gamedataIndex - 10);

                var valuesList = gameDataString.Split(new string[] { "\u0001", "\\lid\\1\\final\\", "\\" }, StringSplitOptions.None);

                var dictionary = new Dictionary<string, string>();

                for (int i = 0; i < valuesList.Length - 1; i += 2)
                {
                    if (i == valuesList.Length - 1)
                        continue;

                    dictionary[valuesList[i]] = valuesList[i + 1];
                }

                if (!dictionary.TryGetValue("Mod", out string v))
                {
                    dictionary.Clear();

                    for (int i = 1; i < valuesList.Length - 1; i += 2)
                    {
                        if (i == valuesList.Length - 1)
                            continue;

                        dictionary[valuesList[i]] = valuesList[i + 1];
                    }
                }

                var mod = dictionary["Mod"];
                var modVersion = dictionary["ModVer"];

                if (!CoreContext.ThunderHawkModManager.ModName.Equals(mod, StringComparison.OrdinalIgnoreCase))
                    return;

                if (!CoreContext.ThunderHawkModManager.ModVersion.Equals(modVersion, StringComparison.OrdinalIgnoreCase))
                    return;

                var playersCount = int.Parse(dictionary["Players"]);

                for (int i = 0; i < playersCount; i++)
                {
                    // Dont process games with AI
                    if (dictionary["PHuman_" + i] != "1")
                    {
                        Logger.Trace($"Stats socket: GAME WITH NONHUMAN PLAYER");
                        return;
                    }
                }

                var gameInternalSession = dictionary["SessionID"];
                var teamsCount = int.Parse(dictionary["Teams"]);
                var version = dictionary["Version"];

                var uniqueGameSessionBuilder = new StringBuilder(gameInternalSession);

                for (int i = 0; i < playersCount; i++)
                {
                    uniqueGameSessionBuilder.Append('<');
                    uniqueGameSessionBuilder.Append(dictionary["player_" + i]);
                    uniqueGameSessionBuilder.Append('>');
                }

                var uniqueSession = uniqueGameSessionBuilder.ToString();

                var players = new PlayerPart[playersCount];

                for (int i = 0; i < players.Length; i++)
                {
                    var player = new PlayerPart();

                    player.Name = dictionary["player_" + i];
                    player.Race = (Race)Enum.Parse(typeof(Race), dictionary["PRace_" + i], true);
                    player.Team = int.Parse(dictionary["PTeam_" + i]);
                    player.FinalState = (PlayerFinalState)Enum.Parse(typeof(PlayerFinalState), dictionary["PFnlState_" + i]);

                    players[i] = player;
                }

                var gameFinishedMessage = new GameFinishedMessage
                {
                    Map = dictionary["Scenario"],
                    SessionId = uniqueSession,
                    Duration = long.Parse(dictionary["Duration"]),
                    ModName = dictionary["Mod"],
                    ModVersion = dictionary["ModVer"],
                    Players = players,
                    IsRateGame = dictionary["Ladder"] == "1"
                };

                CoreContext.MasterServer.SendGameFinishedInfo(gameFinishedMessage);

                DowstatsReplaySender.SendReplay(gameFinishedMessage);

                return;
            }

            if (str.StartsWith(@"\newgame\", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Debugger.Break();
        }

        string GetPidFromInput(string input, int start)
        {
            int end = start;
            while (true)
            {
                var ch = input[end++];

                if (!char.IsDigit(ch))
                    break;
            }

            return input.Substring(start, end - start - 1);
        }

        unsafe void OnChat(TcpPortHandler handler, TcpClientNode node, byte[] buffer, int count)
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

            Logger.Trace(">>>>> " + str);

            var lines = str.Split(ChatSplitChars, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
                HandleChatLine(handler, node, lines[i]);

        }

        unsafe void HandleChatLine(TcpPortHandler handler, TcpClientNode node, string line)
        {
            var values = GetLineValues(line); //line.Split(ChatCommandsSplitChars, StringSplitOptions.RemoveEmptyEntries);

            if (line.StartsWith("LOGIN", StringComparison.OrdinalIgnoreCase)) { HandleLoginCommand(handler, node, values); return; }
            if (line.StartsWith("USRIP", StringComparison.OrdinalIgnoreCase)) { HandleUsripCommand(handler, node, values); return; }
            if (line.StartsWith("CRYPT", StringComparison.OrdinalIgnoreCase)) { HandleCryptCommand(handler, node, values); return; }
            if (line.StartsWith("USER", StringComparison.OrdinalIgnoreCase)) { HandleUserCommand(handler, node, values); return; }
            if (line.StartsWith("NICK", StringComparison.OrdinalIgnoreCase)) { HandleNickCommand(handler, node, values); return; }
            if (line.StartsWith("CDKEY", StringComparison.OrdinalIgnoreCase)) { HandleCdkeyCommand(handler, node, values); return; }
            if (line.StartsWith("JOIN", StringComparison.OrdinalIgnoreCase)) { HandleJoinCommand(handler, node, line, values); return; }
            if (line.StartsWith("MODE", StringComparison.OrdinalIgnoreCase)) { HandleModeCommand(handler, node, values); return; }
            if (line.StartsWith("QUIT", StringComparison.OrdinalIgnoreCase)) { HandleQuitCommand(handler, values); return; }
            if (line.StartsWith("PRIVMSG", StringComparison.OrdinalIgnoreCase)) { HandlePrivmsgCommand(handler, values); return; }
            if (line.StartsWith("SETCKEY", StringComparison.OrdinalIgnoreCase)) { HandleSetckeyCommand(handler, node, line, values); return; }
            if (line.StartsWith("GETCKEY", StringComparison.OrdinalIgnoreCase)) { HandleGetckeyCommand(handler, node, values); return; }
            if (line.StartsWith("TOPIC", StringComparison.OrdinalIgnoreCase)) { HandleTopicCommand(handler, node, values); return; }
            if (line.StartsWith("PART", StringComparison.OrdinalIgnoreCase)) { HandlePartCommand(handler, values); return; }
            if (line.StartsWith("UTM", StringComparison.OrdinalIgnoreCase)) { HandleUtmCommand(handler, line); return; }
            if (line.StartsWith("PING", StringComparison.OrdinalIgnoreCase)) { HandlePingCommand(handler, node, values); return; }

            //REPORT cK ?% localip0 192.168.10.2 localip1 192.168.159.1 localip2 192.168.58.1 localip3 192.168.1.21 localport 6112 natneg 1 statechanged 2 gamename whamdowfram publicip 1304808466 publicport 35108

            Debugger.Break();

            //if (!state.Disposing && state.UserInfo != null)
            //   IrcDaemon.ProcessSocketMessage(state.UserInfo, asciValue);
        }

        void HandlePingCommand(TcpPortHandler handler, TcpClientNode node, string[] values)
        {
            SendToClientChat(node, $":s PONG :s\r\n");
        }

        void HandleUtmCommand(TcpPortHandler handler, string line)
        {
            SteamLobbyManager.SendInLobbyChat(line);
        }

        void HandlePartCommand(TcpPortHandler handler, string[] values)
        {
            //CHATLINE PART #GSP!whamdowfr!Ml39ll1K9M :
            var channelName = values[1];
            
            if (channelName == "#GPG!1")
            {
            }
            else
            {
                if (_gameLaunchReceived)
                { 
                    Logger.Info("PART AFTER GAME LAUNCH");

                    var hash = _enteredLobbyHash;
                    var lobbyId = SteamLobbyManager.CurrentLobbyId;

                    Thread.MemoryBarrier();

                    //Task.Delay(20000).ContinueWith(t =>
                    //{
                        var currentHash = _enteredLobbyHash;
                        var currentLobbyId = SteamLobbyManager.CurrentLobbyId;

                        Thread.MemoryBarrier();

                        _enteredLobbyHash = null;
                        _localServerHash = null;

                        if (hash == currentHash && currentLobbyId == lobbyId)
                            SteamLobbyManager.LeaveFromCurrentLobby();
                    //});
                }
                else
                {
                    _enteredLobbyHash = null;
                    _localServerHash = null;

                    SteamLobbyManager.LeaveFromCurrentLobby();
                }
            }

            _gameLaunchReceived = false;

            var profile = CoreContext.MasterServer.CurrentProfile;

            if (profile == null || !profile.IsProfileActive)
                return;

            SendToClientChat($":{_user} PART {channelName} :Leaving\r\n");
        }

        void HandleTopicCommand(TcpPortHandler handler, TcpClientNode node, string[] values)
        {
            // :Bambochuk2!Xu4FpqOa9X|4@192.168.159.128 TOPIC #GSP!whamdowfr!76561198408785287 :Bambochuk2
            SteamLobbyManager.SetLobbyTopic(values[2]);
            SendToClientChat(node, $":{_user} TOPIC #GSP!whamdowfr!{_enteredLobbyHash} :{values[2]}\r\n");

            //TOPIC #GSP!whamdowfr!Ml39ll1K9M :elamaunt
            /*var channelName = values[1];

            if (channelName.StartsWith("#GSP", StringComparison.OrdinalIgnoreCase))
            {
                var roomHash = channelName.Split('!')[2];

                if (roomHash == _localServerHash)
                {
                    SteamLobbyManager.SetLobbyTopic(values[2]);
                }
            }*/
        }

        string[] GetLineValues(string line)
        {
            var args = new List<string>();
            string prefix;
            string command = string.Empty;

            try
            {
                int i = 0;
                /* This runs in the mainloop :: parser needs to return fast
                 * -> nothing which could block it may be called inside Parser
                 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
                if (line[0] == PrefixCharacter)
                {
                    /* we have a prefix */
                    while (line[++i] != ' ') { }

                    prefix = line.Substring(1, i - 1);
                }
                else
                {
                    prefix = _user;
                }

                int commandStart = i;
                /*command might be numeric (xxx) or command */
                if (char.IsDigit(line[i + 1]) && char.IsDigit(line[i + 2]) && char.IsDigit(line[i + 3]))
                {
                    //replyCode = (ReplyCode)int.Parse(line.Substring(i + 1, 3));
                    i += 4;
                }
                else
                {
                    while ((i < (line.Length - 1)) && line[++i] != ' ') { }

                    if (line.Length - 1 == i) { ++i; }
                    command = line.Substring(commandStart, i - commandStart);
                }

                args.Add(command);

                ++i;
                int paramStart = i;
                while (i < line.Length)
                {
                    if (line[i] == ' ' && i != paramStart)
                    {
                        args.Add(line.Substring(paramStart, i - paramStart));
                        paramStart = i + 1;
                    }
                    if (line[i] == PrefixCharacter)
                    {
                        if (paramStart != i)
                        {
                            args.Add(line.Substring(paramStart, i - paramStart));
                        }
                        args.Add(line.Substring(i + 1));
                        break;
                    }

                    ++i;
                }

                if (i == line.Length)
                {
                    args.Add(line.Substring(paramStart));
                }

            }
            catch (IndexOutOfRangeException)
            {
                Logger.Warn("Invalid Message: " + line);
                // invalid message
            }

            return args.ToArray();
        }

        void HandleSetckeyCommand(TcpPortHandler handler, TcpClientNode node, string line, string[] values)
        {
            var channelName = values[1];

            if (channelName == "#GPG!1")
            {
                var keyValues = values[3];

                var pairs = keyValues.Split(':', '\\');

                if (pairs[1] == "username")
                {
                    SendToClientChat(node, $":s 702 #GPG!1 #GPG!1 {values[2]} BCAST :\\{pairs[1]}\\{pairs[2]}\r\n");
                    return;
                }

                SendToClientChat(node, $":s 702 #GPG!1 #GPG!1 {values[2]} BCAST :\\{pairs[1]}\\{pairs[2]}\r\n");

                CoreContext.MasterServer.SendKeyValuesChanged(_name, pairs.Skip(1).ToArray());

               /* for (int i = 1; i < pairs.Length; i += 2)
                    SendToClientChat($":s 702 #GPG!1 #GPG!1 {values[2]} BCAST :\\{pairs[i]}\\{pairs[i + 1]}\r\n");*/
            }
            else
            {
                if (channelName.StartsWith("#GSP", StringComparison.OrdinalIgnoreCase))
                {
                    //var roomHash = channelName.Split('!')[2];

                    var keyValues = values[3];

                    var pairs = keyValues.Split(':', '\\');

                    // Skip first empty entry
                    for (int i = 1; i < pairs.Length; i += 2)
                        SteamLobbyManager.SetKeyValue(pairs[i], pairs[i + 1]);

                    SteamLobbyManager.SendInLobbyChat(line);
                    HandleRemoteSetckeyCommand(values);
                }
            }
        }

        void HandlePrivmsgCommand(TcpPortHandler handler, string[] values)
        {
            // PRIVMSG #GPG!1 :dfg
            var channelName = values[1];

            if (channelName == "#GPG!1")
            {
                CoreContext.MasterServer.SendChatMessage(values[2], true);
            }
            else
            {
                if (channelName.StartsWith("#GSP", StringComparison.OrdinalIgnoreCase))
                {
                    //var roomHash = channelName.Split('!')[2];

                    // Костыль для работы личных сообщений в игре
                    if (values[1].EndsWith("-thq"))
                        values[1] = values[1].Substring(0, values[1].Length - 4);

                    SteamLobbyManager.SendInLobbyChat(string.Join(" ", values));
                }
            }
        }

        void HandleGetckeyCommand(TcpPortHandler handler, TcpClientNode node, string[] values)
        {
            var channelName = values[1];

            //GETCKEY #GPG!1 * 000 0 :\\username\\b_flags
            var id = values[3];
            var keysString = values[5];

            var keys = keysString.Split(':', '\\');
            var builder = new StringBuilder();

            if (channelName.StartsWith("#GSP", StringComparison.OrdinalIgnoreCase))
            {
                //var roomHash = channelName.Split('!')[2];

                if (!SteamLobbyManager.IsInLobbyNow)
                {
                    for (int k = 0; k < keys.Length; k++)
                    {
                        var key = keys[k];

                        if (string.IsNullOrWhiteSpace(key))
                            continue;

                        string value;

                        if (key == "username")
                            value = _shortUser;
                        else
                            value = SteamLobbyManager.GetLocalMemberData(key);

                        builder.Append(@"\" + value);
                    }

                    SendToClientChat(node, $":s 702 {_name} {channelName} {id} :{builder}\r\n");
                }
                else
                {
                    var count = SteamLobbyManager.GetLobbyMembersCount();

                    for (int i = 0; i < count; i++)
                    {
                        builder.Clear();

                        var name = SteamLobbyManager.GetLobbyMemberName(i);

                        if (name == null)
                            continue;

                        for (int k = 0; k < keys.Length; k++)
                        {
                            var key = keys[k];

                            if (string.IsNullOrWhiteSpace(key))
                                continue;

                            var value = SteamLobbyManager.GetLobbyMemberData(i, key);

                            builder.Append(@"\" + value);
                        }

                        SendToClientChat(node, $":s 702 {_name} {channelName} {name} {id} :{builder}\r\n");
                    }
                }

                SendToClientChat(node, $":s 703 {_name} {channelName} {id} :End of GETCKEY\r\n");
            }
            else
            {
                if (channelName.StartsWith("#GPG", StringComparison.OrdinalIgnoreCase))
                {
                    var users = CoreContext.MasterServer.GetAllUsers();

                    for (int i = 0; i < users.Length; i++)
                    {
                        var user = users[i];

                        if (!user.IsProfileActive)
                            continue;

                        builder.Clear();

                        for (int k = 0; k < keys.Length; k++)
                        {
                            var key = keys[k];

                            if (string.IsNullOrWhiteSpace(key))
                                continue;

                            string value = string.Empty;

                            if (key == "username")
                            {
                                if (user.IsUser)
                                    value = _shortUser;
                                else
                                    value = $"X{GetEncodedIp(user, user.UIName)}X|{user.ActiveProfileId ?? 0}";
                            }

                            if (key == "b_stats")
                                if (user.BStats != null)
                                    value = user.BStats;
                                else
                                    value = $"{user.ActiveProfileId ?? 0}|{user.Score1v1 ?? 1000}|{user.StarsCount}|";
                            if (key == "b_flags")
                                value = user.BFlags ?? string.Empty;

                            builder.Append(@"\" + value);
                        }

                        SendToClientChat(node, $":s 702 {_name} {channelName} {user.UIName} {id} :{builder}\r\n");
                    }

                    SendToClientChat(node, $":s 703 {_name} {channelName} {id} :End of GETCKEY\r\n");
                }
            }
        }

        string GetEncodedIp(UserInfo user, string name)
        {
            // Fake
            var builder = new StringBuilder();

            builder.Append((char)(user.ActiveProfileId ?? 0));

            for (int i = 1; i < 8; i++)
            {
                var ch = name?.ElementAtOrDefault(i);

                if (ch == (char)0)
                    ch = 'a';

                builder.Append(ch);
            }

            return builder.ToString();

            // return $"{(char)((int?)name?.ElementAt(0) + user.ActiveProfileId ?? 0) ?? 'a'}{name?.ElementAt(1) ?? 'b'}{name?.ElementAt(2) ?? 'c'}{name?.ElementAt(3) ?? 'd'}{name?.ElementAt(4) ?? 'e'}{name?.ElementAt(5) ?? 'r'}{name?.ElementAt(6) ?? 't'}{name?.ElementAt(7) ?? 'y'}";
        }

        void HandleQuitCommand(TcpPortHandler handler, string[] values)
        {
            //Restart();
        }

        void HandleModeCommand(TcpPortHandler handler, TcpClientNode node, string[] values)
        {
            var channelName = values[1];

            if (channelName.StartsWith("#GPG", StringComparison.OrdinalIgnoreCase))
            {
                SendToClientChat(node, $":s 324 {_name} {channelName} +\r\n");
            }
            else
            {
                if (channelName.StartsWith("#GSP", StringComparison.OrdinalIgnoreCase))
                {
                    var roomHash = channelName.Split('!')[2];

                    if (_lastLoadedLobbies.TryGetValue(roomHash, out GameServerDetails details))
                    {
                        var maxPLayers = SteamLobbyManager.GetLobbyMaxPlayers();

                        if (maxPLayers == 2 || maxPLayers == 4 || maxPLayers == 6 || maxPLayers == 8)
                            SendToClientChat(node, $":s 324 {_name} {channelName} +l {maxPLayers}\r\n");
                        else
                            SendToClientChat(node, $":s 324 {_name} {channelName} +\r\n");
                    }
                    else
                    {
                        if (roomHash == _localServerHash)
                        {
                            if (values.Length < 4)
                            {
                                var max = SteamLobbyManager.GetLobbyMaxPlayers();

                                if (max > 0 && max < 9)
                                    SendToClientChat(node, $":s 324 {_name} {channelName} +l {max}\r\n");
                                else
                                    SendToClientChat(node, $":s 324 {_name} {channelName} +\r\n");
                            }
                            else
                            {
                                var maxPlayers = values[3];

                                if (int.TryParse(maxPlayers, out int value))
                                    SteamLobbyManager.SetLobbyMaxPlayers(value);

                                SendToClientChat(node, $":{_user} MODE #GSP!whamdowfr!{_enteredLobbyHash} +l {maxPlayers}\r\n");
                            }

                            // CHATLINE MODE #GSP!whamdowfr!Ml39ll1K9M +l 2
                            // CHATLINE MODE #GSP!whamdowfr!Ml39ll1K9M -i-p-s-m-n-t+l+e 2
                        }
                    }
                }
            }
        }

        void HandleJoinCommand(TcpPortHandler handler, TcpClientNode node, string line, string[] values)
        {
            var channelName = values[1];

            _gameLaunchReceived = false;

            if (channelName.StartsWith("#GPG", StringComparison.OrdinalIgnoreCase))
            {
                var users = CoreContext.MasterServer.GetAllUsers();

                var builder = new StringBuilder();

                builder.Append($":{_user} JOIN {channelName}\r\n");
                // SendToClientChat(node, $":{_user} JOIN {channelName}\r\n");
                builder.Append($":s 331 {channelName} :No topic is set\r\n");
               // SendToClientChat(node, $":s 331 {channelName} :No topic is set\r\n");

                _inChat = true;

                var playersList = new StringBuilder();

                for (int i = 0; i < users.Length; i++)
                {
                    var user = users[i];

                    if (!user.IsProfileActive)
                        continue;

                    playersList.Append(user.Name + " ");
                }

                builder.Append($":s 353 {_name} = {channelName} :{playersList}\r\n");
                //SendToClientChat(node, $":s 353 {_name} = {channelName} :{playersList}\r\n");
                builder.Append($":s 366 {_name} {channelName} :End of NAMES list\r\n");
                //SendToClientChat(node, $":s 366 {_name} {channelName} :End of NAMES list\r\n");

                SendToClientChat(builder.ToString());
            }
            else
            {
                if (channelName.StartsWith("#GSP", StringComparison.OrdinalIgnoreCase))
                {
                    var roomHash = channelName.Split('!')[2];

                    if (_lastLoadedLobbies.TryGetValue(roomHash, out GameServerDetails details))
                    {
                        var lobbyId = details.LobbySteamId;
                        SteamLobbyManager.EnterInLobby(lobbyId, _shortUser, _name, _profileId.ToString(), RecreateLobbyToken())
                            .OnFaultOnUi(() =>
                            {
                                SendToClientChat(node, $":{_user} {channelName} :Bad Channel Mask\r\n");
                            })
                            .OnCompletedOnUi(res =>
                            {
                                if (!res)
                                {
                                    SendToClientChat(node, $":{_user} {channelName} :Bad Channel Mask\r\n");
                                    return;
                                }

                                _enteredLobbyHash = details.RoomHash;

                                var playersList = new StringBuilder();

                                var usersInLobby = SteamLobbyManager.GetCurrentLobbyMembers();

                                for (int i = 0; i < usersInLobby.Length; i++)
                                    playersList.Append(usersInLobby[i] + " ");

                                SteamLobbyManager.SendInLobbyChat($"JOIN {_user}");

                                SendToClientChat(node, $":{_user} JOIN {channelName}\r\n");

                                var topic = SteamLobbyManager.GetLobbyTopic() ?? "No topic is set";

                                SendToClientChat(node, $":s 331 {channelName} :{topic}\r\n");
                                SendToClientChat(node, $":s 353 {_name} = {channelName} :@{playersList}\r\n");
                                SendToClientChat(node, $":s 366 {_name} {channelName} :End of NAMES list\r\n");
                            });
                    }
                    else
                    {
                        _localServerHash = roomHash;
                        _enteredLobbyHash = roomHash;

                        var builder = new StringBuilder();

                        builder.Append($":{_user} JOIN {channelName}\r\n");
                        builder.Append($":s 331 {channelName} :No topic is set\r\n");
                        builder.Append($":s 353 {_name} = {channelName} :@{_name}\r\n");
                        builder.Append($":s 366 {_name} {channelName} :End of NAMES list\r\n");

                        SendToClientChat(node, builder.ToString());
                    }
                }
            }
        }

        void HandleCdkeyCommand(TcpPortHandler handler, TcpClientNode node, string[] values)
        {
            SendToClientChat(node, $":s 706 {_name}: 1 :\"Authenticated\"\r\n");
        }

        void HandleUserCommand(TcpPortHandler handler, TcpClientNode node, string[] values)
        {
            _user = $@"{_name}!{values[1]}@{node.RemoteEndPoint?.Address}";
            _shortUser = values[1];
        }

        void HandleNickCommand(TcpPortHandler handler, TcpClientNode node, string[] values)
        {
            var users = CoreContext.MasterServer.GetAllUsers().Count(x => x.IsProfileActive);

            //SendToClientChat($":SERVER!SERVER@* NOTICE {_name} :Authenticated\r\n");
            SendToClientChat(node, $":s 001 {_name} :Welcome to the Matrix {_name}\r\n");
            SendToClientChat(node, $":s 002 {_name} :Your host is xs0, running version 1.0\r\n");
            SendToClientChat(node, $":s 003 {_name} :This server was created Fri Oct 19 1979 at 21:50:00 PDT\r\n");
            SendToClientChat(node, $":s 004 {_name} s 1.0 iq biklmnopqustvhe\r\n");
            SendToClientChat(node, $":s 375 {_name} :- (M) Message of the day - \r\n");
            SendToClientChat(node, $":s 372 {_name} :- Welcome to GameSpy\r\n");
            SendToClientChat(node, $":s 251 :There are {users} users and 0 services on 1 servers\r\n");
            SendToClientChat(node, $":s 252 0 :operator(s)online\r\n");
            SendToClientChat(node, $":s 253 1 :unknown connection(s)\r\n");
            SendToClientChat(node, $":s 254 1 :channels formed\r\n");
            SendToClientChat(node, $":s 255 :I have {users} clients and 1 servers\r\n");
            SendToClientChat(node, $":{_user} NICK {_name}\r\n");
        }

        unsafe void HandleCryptCommand(TcpPortHandler handler, TcpClientNode node, string[] values)
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

            handler.Send(node, DataFunctions.StringToBytes(":s 705 * 0000000000000000 0000000000000000\r\n"));
        }

        void HandleUsripCommand(TcpPortHandler handler, TcpClientNode node, string[] values)
        {
            SendToClientChat(node, $":s 302  :=+@{node.RemoteEndPoint?.Address}\r\n");
        }

        void HandleLoginCommand(TcpPortHandler handler, TcpClientNode node, string[] values)
        {
            var nick = values[2];
            var loginInfo = CoreContext.MasterServer.GetLoginInfo(nick);

            SendToClientChat(node, $":s 707 {nick} 12345678 {loginInfo.ProfileId}\r\n");
            SendToClientChat(node, $":s 687ru: Your languages have been set\r\n");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        void RestartTimer(TcpClientNode node)
        {
            StopTimer();
            _heartbeatState = 0;
            _keepAliveTimer = new Timer(KeepAliveCallback, node, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
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
            if (!_clientManager.Send((TcpClientNode)state, DataFunctions.StringToBytes(@"\ka\\final\")))
            {
                Restart();
                return;
            }

            // every 2nd keep alive request, we send an additional heartbeat
            if ((_heartbeatState & 1) == 0)
            {
                Logger.Trace("sending heartbeat");
                if (!_clientManager.Send((TcpClientNode)state, DataFunctions.StringToBytes(String.Format(@"\lt\{0}\final\", RandomHelper.GetString(22, "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ][") + "__"))))
                {
                    Restart();
                    return;
                }
            }
        }

        public void SendAsServerMessage(string message)
        {
            if (_name == null || !_inChat)
                return;

            SendToClientChat($":SERVER!XaaaaaaaaX|10008@127.0.0.1 PRIVMSG {_name} :{message}\r\n");
        }

        unsafe void SendToClientChat(string message)
        {
            Logger.Trace("<<<<<<<<<<< " + message);

            var bytesToSend = message.ToUTF8Bytes();

            if (_chatEncoded)
                fixed (byte* bytesToSendPtr = bytesToSend)
                    ChatCrypt.GSEncodeDecode(_chatServerKey, bytesToSendPtr, bytesToSend.Length);

            _chat.Send(bytesToSend);
        }

        unsafe void SendToClientChat(TcpClientNode node, string message)
        {
            Logger.Trace("<<<<<<<<<<< " + message);

            var bytesToSend = message.ToUTF8Bytes();

            if (_chatEncoded)
                fixed (byte* bytesToSendPtr = bytesToSend)
                    ChatCrypt.GSEncodeDecode(_chatServerKey, bytesToSendPtr, bytesToSend.Length);

            _chat.Send(node, bytesToSend);
        }

        void OnServerRetrieve(TcpPortHandler handler, TcpClientNode node, byte[] buffer, int count)
        {
            var str = ToASCII(buffer, count);
            Logger.Trace("RETRIEVE " + str);

            var endPoint = node.RemoteEndPoint;

            if (endPoint == null)
            {
                handler.KillClient(node);
                return;
            }

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
                    SendRooms(handler, node, validate);
                    return;
                }
            }

            SteamLobbyManager.LoadLobbies(null, GetIndicator())
               .ContinueWith(task =>
               {
                   try
                   {
                       if (task.Status != TaskStatus.RanToCompletion)
                       {
                           Console.WriteLine(task.Exception);
                           return;
                       }

                       // var currentRating = ServerContext.ChatServer.CurrentRating;

                       var servers = task.Result;

                       for (int i = 0; i < servers.Length; i++)
                       {
                           var server = servers[i];
                           server["score_"] = GetCurrentRating(server.MaxPlayers);
                       }

                       var fields = data[5].Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

                       var unencryptedBytes = ParseHelper.PackServerList(endPoint, servers, fields, isAutomatch);

                       _lastLoadedLobbies.Clear();

                       for (int i = 0; i < servers.Length; i++)
                       {
                           var server = servers[i];
                           var retranslationPort = ushort.Parse(server.HostPort);
                           var channelHash = ChatCrypt.PiStagingRoomHash("127.0.0.1", "127.0.0.1", retranslationPort);

                           Logger.Info($"HASHFOR 127.0.0.1:{retranslationPort}  {channelHash}");

                           server.RoomHash = channelHash;
                           _lastLoadedLobbies[channelHash] = server;
                       }

                       Logger.Info("SERVERS VALIDATE VALUE ~" + validate+"~");
                       var encryptedBytes = GSEncoding.Encode("pXL838".ToAssciiBytes(), DataFunctions.StringToBytes(validate), unencryptedBytes, unencryptedBytes.LongLength);

                       Logger.Info("SERVERS bytes "+ encryptedBytes.Length);

                       handler.Send(node, encryptedBytes);

                       LobbiesUpdatedByRequest?.Invoke(servers.Select(x => new GameHostInfo()
                       {
                           IsUser = x.HostSteamId == SteamUser.GetSteamID(),
                           MaxPlayers = x.MaxPlayers.ParseToIntOrDefault(),
                           Players = x.PlayersCount.ParseToIntOrDefault(),
                           Ranked = x.Ranked,
                           GameVariant = x.GameVariant,
                           Teamplay = x.IsTeamplay
                       }).ToArray());
                   }
                   finally
                   {
                       handler.KillClient(node);
                   }
               }).Wait();
        }

        string GetCurrentRating(string maxPlayers)
        {
            var loginInfo = CoreContext.MasterServer.GetLoginInfo(_name);

            if (loginInfo == null)
                return "0";

            var stats = CoreContext.MasterServer.GetStatsInfo(loginInfo.ProfileId);

            if (stats == null)
                return "0";

            switch (maxPlayers)
            {
                case "2": return stats.Score1v1.ToString();
                case "4": return stats.Score2v2.ToString();
                case "6": return stats.Score3v3_4v4.ToString();
                case "8": return stats.Score3v3_4v4.ToString();
                default: return "0";
            }
        }

        void SendRooms(TcpPortHandler handler, TcpClientNode node, string validate)
        {
            var bytes = new List<byte>();

            //var remoteEndPoint = handler.RemoteEndPoint;
            //bytes.AddRange(remoteEndPoint.Address.GetAddressBytes());
            bytes.AddRange(IPAddress.Loopback.GetAddressBytes());

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

            // for (int i = 1; i <= 10; i++)
            // {
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
            bytes.AddRange(DataFunctions.StringToBytes(CoreContext.MasterServer.GetAllUsers().Count(x => x.IsProfileActive).ToString()));
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
            // }

            bytes.AddRange(new byte[] { 0, 255, 255, 255, 255 });

            var array = bytes.ToArray();

            byte[] enc = GSEncoding.Encode("pXL838".ToAssciiBytes(), DataFunctions.StringToBytes(validate), array, array.LongLength);

            handler.Send(node, enc);

            handler.KillClient(node);
        }

        void OnServerReport(UdpPortHandler handler, byte[] receivedBytes, IPEndPoint remote)
        {
            var str = ToUtf8(receivedBytes, receivedBytes.Length);
            Logger.Trace("REPORT " + str);

            if (receivedBytes[0] == (byte)MessageType.AVAILABLE)
            {
                Logger.Trace("REPORT: Send AVAILABLE");
                handler.Send(new byte[] { 0xfe, 0xfd, 0x09, 0x00, 0x00, 0x00, 0x00 }, remote);
            }
            else if (receivedBytes.Length > 5 && receivedBytes[0] == (byte)MessageType.HEARTBEAT)
            {
                // this is where server details come in, it starts with 0x03, it happens every 60 seconds or so

                var receivedData = Encoding.UTF8.GetString(receivedBytes.Skip(5).ToArray());
                var sections = receivedData.Split(new string[] { "\x00\x00\x00", "\x00\x00\x02" }, StringSplitOptions.None);

                if (sections.Length != 3 && !receivedData.EndsWith("\x00\x00"))
                    return; // true means we don't send back a response
                
                if (!_challengeResponded)
                {
                    byte[] uniqueId = new byte[4];
                    Array.Copy(receivedBytes, 1, uniqueId, 0, 4);

                    byte[] response = new byte[] { 0xfe, 0xfd, (byte)MessageType.CHALLENGE_RESPONSE, uniqueId[0], uniqueId[1], uniqueId[2], uniqueId[3], 0x41, 0x43, 0x4E, 0x2B, 0x78, 0x38, 0x44, 0x6D, 0x57, 0x49, 0x76, 0x6D, 0x64, 0x5A, 0x41, 0x51, 0x45, 0x37, 0x68, 0x41, 0x00 };

                    Logger.Trace("REPORT: Send challenge responce");
                    handler.Send(response, remote);
                    _challengeResponded = true;
                }
                else
                {
                    string serverVars = sections[0];
                    //string playerVars = sections[1];
                    //string teamVars = sections[2];

                    var details = ParseHelper.ParseDetails(serverVars);

                    if (details.StateChanged == "2")
                    {
                        Logger.Trace("REPORT: ClearServerDetails");
                        SteamLobbyManager.SetLobbyJoinable(false);
                        _challengeResponded = false;
                        _localServer = null;
                    }
                    else
                    {
                        Logger.Trace("REPORT: UpdateCurrentLobby");

                        details["IPAddress"] = remote.Address.ToString();
                        details["QueryPort"] = remote.Port.ToString();
                        details["LastRefreshed"] = DateTime.UtcNow.ToString();
                        details["LastPing"] = DateTime.UtcNow.ToString();
                        details["country"] = "??";
                        details["hostport"] = remote.Port.ToString();
                        details["localport"] = remote.Port.ToString();

                        SteamLobbyManager.UpdateCurrentLobby(details, GetIndicator());

                        _localServer = details;

                        if (details.IsValid && details.Ranked)
                            CoreContext.MasterServer.RequestGameBroadcast(details.IsTeamplay, details.GameVariant, details.MaxPlayers.ParseToIntOrDefault(), details.PlayersCount.ParseToIntOrDefault(), details.Ranked);
                    }
                }
            }
            else if (receivedBytes.Length > 5 && receivedBytes[0] == (byte)MessageType.CHALLENGE_RESPONSE)
            {
                Logger.Trace("REPORT: Validate challenge responce");

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

                    var token = default(CancellationToken); //RecreateLobbyToken();
                    //token.Register(() => SteamLobbyManager.LeaveFromCurrentLobby());
                   
                    handler.Send(response, remote);

                    if (!SteamLobbyManager.IsInLobbyNow)
                        SteamLobbyManager.CreatePublicLobby(token, _name, _shortUser, _flags, GetIndicator())/*.OnCompletedOnUi(lobbyId =>
                        {
                            //if (token.IsCancellationRequested)
                            //    return;
                            //handler.Send(response, remote);
                        })*/.Wait();
                }
                else
                {
                    Logger.Trace("REPORT: Validation failed");
                }
            }
            else if (receivedBytes.Length == 5 && receivedBytes[0] == (byte)MessageType.KEEPALIVE)
            {
                // this is a server ping, it starts with 0x08, it happens every 20 seconds or so
                Logger.Trace("REPORT: KEEPALIVE");

                byte[] uniqueId = new byte[4];
                Array.Copy(receivedBytes, 1, uniqueId, 0, 4);
                RefreshServerPing(remote);
            }
        }

        CancellationToken RecreateLobbyToken()
        {
            return (_lobbyTokenSource = _lobbyTokenSource.Recreate()).Token;
        }

        void RefreshServerPing(IPEndPoint remote)
        {
            // TODO
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Stop()
        {
            _gameLaunchReceived = false;
            _challengeResponded = false;
            _inChat = false;
            _enteredLobbyHash = null;
            _localServerHash = null;
            _localServer = null;

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

        string DecryptPassword(string password)
        {
            string decrypted = GsBase64Decode(password, password.Length);
            GsEncode(ref decrypted);
            return decrypted;
        }

        int GsEncode(ref string password)
        {
            byte[] pass = DataFunctions.StringToBytes(password);

            int i;
            int a;
            int c;
            int d;
            int num = 0x79707367;   // "gspy"
            int passlen = pass.Length;

            if (num == 0)
                num = 1;
            else
                num &= 0x7fffffff;

            for (i = 0; i < passlen; i++)
            {
                d = 0xff;
                c = 0;
                d -= c;
                if (d != 0)
                {
                    num = GsLame(num);
                    a = num % d;
                    a += c;
                }
                else
                    a = c;

                pass[i] ^= (byte)(a % 256);
            }

            password = DataFunctions.BytesToString(pass);
            return passlen;
        }

        int GsLame(int num)
        {
            int a;
            int c = (num >> 16) & 0xffff;

            a = num & 0xffff;
            c *= 0x41a7;
            a *= 0x41a7;
            a += ((c & 0x7fff) << 16);

            if (a < 0)
            {
                a &= 0x7fffffff;
                a++;
            }

            a += (c >> 15);

            if (a < 0)
            {
                a &= 0x7fffffff;
                a++;
            }

            return a;
        }
        string GsBase64Decode(string s, int size)
        {
            byte[] data = DataFunctions.StringToBytes(s);

            int len;
            int xlen;
            int a = 0;
            int b = 0;
            int c = 0;
            int step;
            int limit;
            int y = 0;
            int z = 0;

            byte[] buff;
            byte[] p;

            char[] basechars = new char[128]
            {   // supports also the Gamespy base64
				'\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00',
                '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00',
                '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x3e', '\x00', '\x00', '\x00', '\x3f',
                '\x34', '\x35', '\x36', '\x37', '\x38', '\x39', '\x3a', '\x3b', '\x3c', '\x3d', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00',
                '\x00', '\x00', '\x01', '\x02', '\x03', '\x04', '\x05', '\x06', '\x07', '\x08', '\x09', '\x0a', '\x0b', '\x0c', '\x0d', '\x0e',
                '\x0f', '\x10', '\x11', '\x12', '\x13', '\x14', '\x15', '\x16', '\x17', '\x18', '\x19', '\x3e', '\x00', '\x3f', '\x00', '\x00',
                '\x00', '\x1a', '\x1b', '\x1c', '\x1d', '\x1e', '\x1f', '\x20', '\x21', '\x22', '\x23', '\x24', '\x25', '\x26', '\x27', '\x28',
                '\x29', '\x2a', '\x2b', '\x2c', '\x2d', '\x2e', '\x2f', '\x30', '\x31', '\x32', '\x33', '\x00', '\x00', '\x00', '\x00', '\x00'
            };

            if (size <= 0)
                len = data.Length;
            else
                len = size;

            xlen = ((len >> 2) * 3) + 1;
            buff = new byte[xlen % 256];
            if (buff.Length == 0) return null;

            p = buff;
            limit = data.Length + len;

            for (step = 0; ; step++)
            {
                do
                {
                    if (z >= limit)
                    {
                        c = 0;
                        break;
                    }
                    if (z < data.Length)
                        c = data[z];
                    else
                        c = 0;
                    z++;
                    if ((c == '=') || (c == '_'))
                    {
                        c = 0;
                        break;
                    }
                } while (c != 0 && ((c <= (byte)' ') || (c > 0x7f)));
                if (c == 0) break;

                switch (step & 3)
                {
                    case 0:
                        a = basechars[c];
                        break;
                    case 1:
                        b = basechars[c];
                        p[y++] = (byte)(((a << 2) | (b >> 4)) % 256);
                        break;
                    case 2:
                        a = basechars[c];
                        p[y++] = (byte)((((b & 15) << 4) | (a >> 2)) % 256);
                        break;
                    case 3:
                        p[y++] = (byte)((((a & 3) << 6) | basechars[c]) % 256);
                        break;
                    default:
                        break;
                }
            }
            p[y] = 0;

            len = p.Length - buff.Length;

            if (size != 0)
                size = len;

            if ((len + 1) != xlen)
                if (buff.Length == 0) return null;

            return DataFunctions.BytesToString(buff).Substring(0, y);
        }

        byte[] XorBytes(byte[] data, int start, int count, string keystr)
        {
            byte[] key = Encoding.ASCII.GetBytes(keystr);

            for (int i = start; i < count; i++)
                data[i] = (byte)(data[i] ^ key[i % key.Length]);

            return data;
        }

        byte[] XorBytes(string str, string keystr, int lengthOffset = 0)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            byte[] key = Encoding.UTF8.GetBytes(keystr);

            var length = data.Length - lengthOffset;

            for (int i = 0; i < length; i++)
                data[i] = (byte)(data[i] ^ key[i % key.Length]);

            return data;
        }

        public string GetIndicator()
        {
#if SPACEWAR
            return "SOULSTORM";
#else
            return CoreContext.ThunderHawkModManager.ModName + " "+ CoreContext.ThunderHawkModManager.ModVersion;
#endif
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
}";

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
