using GSMasterServer.Data;
using GSMasterServer.Utils;
using IrcD.Core;
using IrcD.Core.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GSMasterServer.Servers
{
    internal class StatsServer : Server
    {
        const string Category = "Stats";

        const string XorKEY = "GameSpy3D";

        Thread _thread;
        Socket _newPeerAceptingsocket;
        readonly ManualResetEvent _reset = new ManualResetEvent(false);
        readonly IrcDaemon _ircDaemon;

        Dictionary<long, SocketState> PlayerEndPoints = new Dictionary<long, SocketState>();

        public StatsServer(IPAddress address, ushort port)
        {
            _thread = new Thread(StartServer)
            {
                Name = "Stats Socket Thread"
            };

            _thread.Start(new AddressInfo()
            {
                Address = address,
                Port = port
            });

            _ircDaemon = new IrcDaemon(IrcMode.Modern);
        }

        private void StartServer(object parameter)
        {
            AddressInfo info = (AddressInfo)parameter;

            Log(Category, "Init");

            try
            {
                _newPeerAceptingsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = 5000,
                    ReceiveTimeout = 5000,
                    SendBufferSize = 65535,
                    ReceiveBufferSize = 65535
                };

                _newPeerAceptingsocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
                _newPeerAceptingsocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

                _newPeerAceptingsocket.Bind(new IPEndPoint(info.Address, info.Port));
                _newPeerAceptingsocket.Listen(10);

            }
            catch (Exception e)
            {
                LogError(Category, String.Format("Unable to bind Server List Retrieval to {0}:{1}", info.Address, info.Port));
                LogError(Category, e.ToString());
                return;
            }

            while (true)
            {
                _reset.Reset();
                _newPeerAceptingsocket.BeginAccept(AcceptCallback, _newPeerAceptingsocket);
                _reset.WaitOne();
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            _reset.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket socket = listener.EndAccept(ar);

            SocketState state = new SocketState()
            {
                Socket = socket
            };
            
            socket.Send(XorBytes(@"\lc\1\challenge\KNDVKXFQWP\id\1\final\", XorKEY, 7));
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

                LogError(Category, "Error receiving data");
                LogError(Category, String.Format("{0} {1}", e.SocketErrorCode, e));
                return;
            }
        }



        private unsafe void OnDataReceived(IAsyncResult async)
        {
            SocketState state = (SocketState)async.AsyncState;

            if (state == null || state.Socket == null || !state.Socket.Connected)
                return;

            try
            {
                // receive data from the socket
                int received = state.Socket.EndReceive(async);

                if (received == 0)
                    return;

                var buffer = state.Buffer;

                var input = Encoding.UTF8.GetString(XorBytes(buffer, 0, received - 7, XorKEY), 0, received);

                Log(Category, input);

                if (input.StartsWith(@"\auth\\gamename\"))
                {
                    SendToClient(state, @"\lc\2\sesskey\1482017401\proof\0\id\1\final\");

                    goto CONTINUE;
                }

                if (input.StartsWith(@"\authp\\pid\"))
                {
                    //var clientData = LoginDatabase.Instance.GetData(state.Name);


                    // \authp\\pid\87654321\resp\67512e365ba89497d60963caa4ce23d4\lid\1\final\

                    // \authp\\pid\87654321\resp\7e2270c581e8daf5a5321ff218953035\lid\1\final\

                    var pid = input.Substring(12, 9);
                    
                    state.PlayerId = long.Parse(pid);

                    SendToClient(state, $@"\pauthr\{pid}\lid\1\final\");
                    //SendToClient(state, @"\pauthr\-3\lid\1\errmsg\helloworld\final\");
                    //SendToClient(state, @"\pauthr\100000004\lid\1\final\");

                    goto CONTINUE;
                }

                
                if (input.StartsWith(@"\getpd\"))
                {
                    // \\getpd\\\\pid\\87654321\\ptype\\3\\dindex\\0\\keys\\\u0001points\u0001points2\u0001points3\u0001stars\u0001games\u0001wins\u0001disconn\u0001a_durat\u0001m_streak\u0001f_race\u0001SM_wins\u0001Chaos_wins\u0001Ork_wins\u0001Tau_wins\u0001SoB_wins\u0001DE_wins\u0001Eldar_wins\u0001IG_wins\u0001Necron_wins\u0001lsw\u0001rnkd_vics\u0001con_rnkd_vics\u0001team_vics\u0001mdls1\u0001mdls2\u0001rg\u0001pw\\lid\\1\\final\\
                    // \getpd\\pid\87654321\ptype\3\dindex\0\keys\pointspoints2points3starsgameswinsdisconna_duratm_streakf_raceSM_winsChaos_winsOrk_winsTau_winsSoB_winsDE_winsEldar_winsIG_winsNecron_winslswrnkd_vicscon_rnkd_vicsteam_vicsmdls1mdls2rgpw\lid\1\final\
                    var pid = input.Substring(12, 9);

                    var keysIndex = input.IndexOf("keys") + 5;
                    var keys = input.Substring(keysIndex);
                    var keysList = keys.Split(new string[] { "\u0001", "\\lid\\1\\final\\", "final", "\\", "lid" }, StringSplitOptions.RemoveEmptyEntries );

                    var keysResult = new StringBuilder();
                    var stats = UsersDatabase.Instance.GetStatsDataByProfileId(long.Parse(pid));

                    var gamesCount = stats.GamesCount;
                    var stars = Math.Min(5, gamesCount);



                    for (int i = 0; i < keysList.Length; i++)
                    {
                        var key = keysList[i];

                        keysResult.Append("\\"+key+"\\");
                        
                        switch (key)
                        {
                            case "points": keysResult.Append(stats.Score1v1); break;
                            case "points2": keysResult.Append(stats.Score2v2); break;
                            case "points3": keysResult.Append(stats.Score3v3); break;

                            case "stars": keysResult.Append(stars); break;

                            case "games": keysResult.Append(gamesCount); break;
                            case "wins": keysResult.Append(stats.WinsCount); break;
                            case "disconn": keysResult.Append(stats.Disconnects); break;
                            case "a_durat": keysResult.Append(stats.AverageDurationTicks); break;
                            case "m_streak": keysResult.Append(stats.Winstreak); break;

                            case "f_race": keysResult.Append(stats.FavouriteRace); break;

                            case "SM_wins": keysResult.Append(stats.Smwincount); break;
                            case "Chaos_wins": keysResult.Append(stats.Csmwincount); break;
                            case "Ork_wins": keysResult.Append(stats.Orkwincount); break;
                            case "Tau_wins": keysResult.Append(stats.Tauwincount); break;
                            case "SoB_wins": keysResult.Append(stats.Sobwincount); break;
                            case "DE_wins": keysResult.Append(stats.Dewincount); break;
                            case "Eldar_wins": keysResult.Append(stats.Eldarwincount); break;
                            case "IG_wins": keysResult.Append(stats.Igwincount); break;
                            case "Necron_wins": keysResult.Append(stats.Necrwincount); break;
                            /*case "lsw": keysResult.Append("123"); break;
                            case "rnkd_vics": keysResult.Append("50"); break;
                            case "con_rnkd_vics": keysResult.Append("200"); break;
                            case "team_vics": keysResult.Append("250"); break;
                            case "mdls1": keysResult.Append("260"); break;
                            case "mdls2": keysResult.Append("270"); break;
                            case "rg": keysResult.Append("280"); break;
                            case "pw": keysResult.Append("290"); break;*/
                            default:
                                keysResult.Append("0");
                                break;
                        }

                    }

                    SendToClient(state, $@"\getpdr\1\lid\1\pid\{pid}\mod\{stats.Modified}\length\{keys.Length}\data\{keysResult}\final\");
                    
                    goto CONTINUE;
                }
                
                if (input.StartsWith(@"\setpd\"))
                {
                    var pid = input.Substring(12, 9);

                    var lidIndex = input.IndexOf("\\lid\\", StringComparison.OrdinalIgnoreCase);
                    var lid = input.Substring(lidIndex+5, 1);

                    var timeInSeconds = (ulong)((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);

                    SendToClient(state, $@"\setpdr\1\lid\{lid}\pid\{pid}\mod\{timeInSeconds}\final\");

                    goto CONTINUE;
                }

                if (input.StartsWith(@"\updgame\"))
                {
                    var gamedataIndex = input.IndexOf("gamedata");
                    var finalIndex = input.IndexOf("final");

                    var gameDataString = input.Substring(gamedataIndex + 9, finalIndex - gamedataIndex - 10);

                    var valuesList = gameDataString.Split(new string[] { "\u0001", "\\lid\\1\\final\\", "final", "\\", "lid" }, StringSplitOptions.RemoveEmptyEntries);

                    var dictionary = new Dictionary<string, string>();

                    for (int i = 0; i < valuesList.Length; i+=2)
                        dictionary[valuesList[i]] = valuesList[i + 1];

                    var playersCount = int.Parse(dictionary["Players"]);

                    for (int i = 0; i < playersCount; i++)
                    {
                        // Dont process games with AI
                        if (dictionary["PHuman_" + i] != "1")
                            goto CONTINUE;
                    }

                    var teamsCount = int.Parse(dictionary["Teams"]);
                    var version = dictionary["Version"];
                    var mod = dictionary["Mod"];
                    var modVersion = dictionary["ModVer"];
                    
                    var usersGameInfos = new GameUserInfo[playersCount];

                    GameUserInfo winnerInfo = null;

                    for (int i = 0; i < playersCount; i++)
                    {
                        //var nick = dictionary["player_"+i];
                        var pid = long.Parse(dictionary["PID_" + i]);

                        var info = new GameUserInfo()
                        {
                            Stats = UsersDatabase.Instance.GetStatsDataByProfileId(pid),
                            Race = Enum.Parse<Race>(dictionary["PRace_" + i], true),
                            Team = int.Parse(dictionary["PTeam_" + i])
                        };

                        usersGameInfos[i] = info;
                        if (pid == state.PlayerId)
                            winnerInfo = info;
                    }
                    
                    var teams = usersGameInfos.GroupBy(x => x.Team).ToDictionary(x => x.Key, x => x.ToArray());
                    var winnerTeam = teams[winnerInfo.Team];
                    
                    foreach (var team in teams)
                    {
                        for (int i = 0; i < team.Value.Length; i++)
                        {
                            var info = team.Value[i];

                            switch (info.Race)
                            {
                                case Race.space_marine_race:
                                    info.Stats.Smgamescount++;
                                    break;
                                case Race.chaos_space_marine_race:
                                    info.Stats.Csmgamescount++;
                                    break;
                                case Race.ork_race:
                                    info.Stats.Orkgamescount++;
                                    break;
                                case Race.eldar_race:
                                    info.Stats.Eldargamescount++;
                                    break;
                                case Race.imperial_guard_race:
                                    info.Stats.Iggamescount++;
                                    break;
                                case Race.necron_race:
                                    info.Stats.Necrgamescount++;
                                    break;
                                case Race.tau_race:
                                    info.Stats.Taugamescount++;
                                    break;
                                case Race.dark_eldar_race:
                                    info.Stats.Degamescount++;
                                    break;
                                case Race.sisters_race:
                                    info.Stats.Sobgamescount++;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    for (int i = 0; i < winnerTeam.Length; i++)
                    {
                        var info = winnerTeam[i];

                        switch (info.Race)
                        {
                            case Race.space_marine_race:
                                info.Stats.Smwincount++;
                                break;
                            case Race.chaos_space_marine_race:
                                info.Stats.Csmwincount++;
                                break;
                            case Race.ork_race:
                                info.Stats.Orkwincount++;
                                break;
                            case Race.eldar_race:
                                info.Stats.Eldarwincount++;
                                break;
                            case Race.imperial_guard_race:
                                info.Stats.Igwincount++;
                                break;
                            case Race.necron_race:
                                info.Stats.Necrwincount++;
                                break;
                            case Race.tau_race:
                                info.Stats.Tauwincount++;
                                break;
                            case Race.dark_eldar_race:
                                info.Stats.Dewincount++;
                                break;
                            case Race.sisters_race:
                                info.Stats.Sobwincount++;
                                break;
                            default:
                                break;
                        }
                    }

                    var chatUserInfo = ChatServer.IrcDaemon.Users[state.PlayerId];
                    var game = chatUserInfo.Game;
                    // For rated games
                    if (game != null && game.Clean())
                    {
                        //var ratingGameType = 

                        //var teamsAverageRatings = usersGameInfos.GroupBy(x => x.Team).ToDictionary(x => x.First().Team, x => x.Average(y => y.Stats.Score1v1));

                    }

                    for (int i = 0; i < usersGameInfos.Length; i++)
                        UsersDatabase.Instance.UpdateUserStats(usersGameInfos[i].Stats);

                    // Custom game
                    /*
                        [0]: "PHuman_0"
                        [1]: "1"
                        [2]: "SessionID"
                        [3]: "-94568309"
                        [4]: "WinBy"
                        [5]: "ANNIHILATE"
                        [6]: "PHuman_1"
                        [7]: "1"
                        [8]: "Ladder"
                        [9]: "0"
                        [10]: "player_0"
                        [11]: "sF|elamaunt"
                        [12]: "player_1"
                        [13]: "Bambochuk"
                        [14]: "PTeam_0"
                        [15]: "1"
                        [16]: "PTeam_1"
                        [17]: "0"
                        [18]: "Players"
                        [19]: "2"
                        [20]: "Teams"
                        [21]: "2"
                        [22]: "Version"
                        [23]: "1.2.120"
                        [24]: "ctime_0"
                        [25]: "0"
                        [26]: "ctime_1"
                        [27]: "0"
                        [28]: "Scenario"
                        [29]: "2P_BATTLE_MARSHES"
                        [30]: "Mod"
                        [31]: "dxp2"
                        [32]: "PFnlState_0"
                        [33]: "5"
                        [34]: "PTtlSc_0"
                        [35]: "8"
                        [36]: "PFnlState_1"
                        [37]: "0"
                        [38]: "PRace_0"
                        [39]: "sisters_race"
                        [40]: "PTtlSc_1"
                        [41]: "8"
                        [42]: "PRace_1"
                        [43]: "ork_race"
                        [44]: "PID_0"
                        [45]: "100000001"
                        [46]: "ModVer"
                        [47]: "1.0"
                        [48]: "PID_1"
                        [49]: "100000002"
                        [50]: "Duration"
                        [51]: "4"
                        */

                    // Ladder
                    /*[0]: "PHuman_0"
                      [1]: "1"
                      [2]: "SessionID"
                      [3]: "-248966396"
                      [4]: "WinBy"
                      [5]: "ANNIHILATE"
                      [6]: "PHuman_1"
                      [7]: "1"
                      [8]: "Ladder"
                      [9]: "0"
                      [10]: "player_0"
                      [11]: "Bambochuk"
                      [12]: "player_1"
                      [13]: "sF|elamaunt"
                      [14]: "PTeam_0"
                      [15]: "0"
                      [16]: "PTeam_1"
                      [17]: "1"
                      [18]: "Players"
                      [19]: "2"
                      [20]: "Teams"
                      [21]: "2"
                      [22]: "Version"
                      [23]: "1.2.120"
                      [24]: "ctime_0"
                      [25]: "0"
                      [26]: "ctime_1"
                      [27]: "0"
                      [28]: "Scenario"
                      [29]: "2P_TITAN_FALL"
                      [30]: "Mod"
                      [31]: "dxp2"
                      [32]: "PFnlState_0"
                      [33]: "0"
                      [34]: "PTtlSc_0"
                      [35]: "657"
                      [36]: "PFnlState_1"
                      [37]: "5"
                      [38]: "PRace_0"
                      [39]: "ork_race"
                      [40]: "PTtlSc_1"
                      [41]: "658"
                      [42]: "PRace_1"
                      [43]: "dark_eldar_race"
                      [44]: "PID_0"
                      [45]: "100000002"
                      [46]: "ModVer"
                      [47]: "1.0"
                      [48]: "PID_1"
                      [49]: "100000001"
                      [50]: "Duration"
                      [51]: "329"
                      */

                }

                // \newgame\\connid\1482017401\sesskey\43152578\final\
                // \updgame\\sesskey\43152578\done\1\gamedata\PHuman_01SessionID-1909266334WinByANNIHILATEPHuman_11Ladder1player_0elamauntplayer_1sF|elamauntPTeam_00PTeam_11Players2Teams2Version1.2ctime_00ctime_10Scenario2P_TRANQUILITYS_ENDModdxp2PFnlState_03PTtlSc_0809PFnlState_15PRace_0necron_racePTtlSc_14310PRace_1tau_racePID_017972147ModVer1.0PID_135226254Duration409\final\
                // \\newgame\\\\connid\\1482017401\\sesskey\\147427625\\final\\
                // \\updgame\\\\sesskey\\147427625\\connid\\1482017401\\done\\1\\gamedata\\\u0001PHuman_0\u00011\u0001SessionID\u00011101289433\u0001WinBy\u0001ANNIHILATE\u0001PHuman_1\u00011\u0001Ladder\u00010\u0001player_0\u0001sF|elamaunt\u0001player_1\u0001Bambochuk\u0001PTeam_0\u00011\u0001PTeam_1\u00010\u0001Players\u00012\u0001Teams\u00012\u0001Version\u00011.2.120\u0001ctime_0\u00010\u0001ctime_1\u00010\u0001Scenario\u00012P_MEETING_OF_MINDS\u0001Mod\u0001dxp2\u0001PFnlState_0\u00010\u0001PTtlSc_0\u000173\u0001PFnlState_1\u00015\u0001PRace_0\u0001dark_eldar_race\u0001PTtlSc_1\u000173\u0001PRace_1\u0001ork_race\u0001PID_0\u0001100000001\u0001ModVer\u00011.0\u0001PID_1\u0001100000002\u0001Duration\u000136\\final\\
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
            CONTINUE: WaitForData(state);
        }

        private unsafe int SendToClient(object abstractState, string message)
        {
            var state = (SocketState)abstractState;

            Log("StatsRESP", message);
            
            var bytesToSend = XorBytes(message, XorKEY, 7);

            SendToClient(ref state, bytesToSend);
            return bytesToSend.Length;
        }

        bool SendToClient(ref SocketState state, byte[] data)
        {
            if (data == null)
                return false;
            
            try
            {
                var res = state.Socket.BeginSend(data, 0, data.Length, SocketFlags.None, OnSent, state);
                return true;
            }
            catch (NullReferenceException)
            {
                return false;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.ConnectionAborted &&
                    e.SocketErrorCode != SocketError.ConnectionReset)
                {
                    LogError(Category, "Error sending data");
                    LogError(Category, String.Format("{0} {1}", e.SocketErrorCode, e));
                }
                
                return false;
            }
        }

        void OnSent(IAsyncResult async)
        {
            SocketState state = (SocketState)async.AsyncState;

            if (state == null || state.Socket == null)
                return;

            try
            {
                var remote = (IPEndPoint)state.Socket.RemoteEndPoint;
                int sent = state.Socket.EndSend(async);
               // Log(Category, String.Format("[{0}] Sent {1} byte response to: {2}:{3}", Category, sent, remote.Address, remote.Port));
            }
            catch (NullReferenceException)
            {
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

        private class SocketState : IDisposable
        {
            public Socket Socket = null;
            public byte[] Buffer = new byte[8192];
            public long PlayerId;

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

        private class GameUserInfo
        {
            public StatsData Stats;
            public Race Race;
            public int Team;
        }
    }
}
