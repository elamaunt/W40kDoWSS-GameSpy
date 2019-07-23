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
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GSMasterServer.Servers
{
    internal class StatsServer : Server
    {
        const string Category = "Stats";

        const string XorKEY = "GameSpy3D";

        long _sessionCounter;

        Thread _thread;
        Socket _newPeerAceptingsocket;
        readonly ManualResetEvent _reset = new ManualResetEvent(false);
        readonly IrcDaemon _ircDaemon;

        readonly MemoryCache HandledGamesCache = new MemoryCache("GameIds");

        readonly ConcurrentDictionary<string, DateTime> HandledGames = new ConcurrentDictionary<string, DateTime>();

        readonly ConcurrentDictionary<long, SocketState> PlayerEndPoints = new ConcurrentDictionary<long, SocketState>();

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
                //_newPeerAceptingsocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

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
                    var sesskey = Interlocked.Increment(ref _sessionCounter).ToString("0000000000");

                    SendToClient(state, $@"\lc\2\sesskey\{sesskey}\proof\0\id\1\final\");

                    goto CONTINUE;
                }

                if (input.StartsWith(@"\authp\\pid\"))
                {
                    //var clientData = LoginDatabase.Instance.GetData(state.Name);


                    // \authp\\pid\87654321\resp\67512e365ba89497d60963caa4ce23d4\lid\1\final\

                    // \authp\\pid\87654321\resp\7e2270c581e8daf5a5321ff218953035\lid\1\final\

                    var pid = input.Substring(12, 9);
                    
                    state.ProfileId = long.Parse(pid);

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
                            case "a_durat": keysResult.Append(stats.AverageDuractionTicks); break;
                            case "m_streak": keysResult.Append(stats.Best1v1Winstreak); break;

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

                    var gameInternalSession = dictionary["SessionID"];
                    var teamsCount = int.Parse(dictionary["Teams"]);
                    var version = dictionary["Version"];
                    var mod = dictionary["Mod"];
                    var modVersion = dictionary["ModVer"];
                    

                    var uniqueGameSessionBuilder = new StringBuilder(gameInternalSession);

                    for (int i = 0; i < playersCount; i++)
                    {
                        uniqueGameSessionBuilder.Append('<');
                        uniqueGameSessionBuilder.Append(dictionary["PID_" + i]);
                        uniqueGameSessionBuilder.Append('>');
                    }

                    var uniqueSession = uniqueGameSessionBuilder.ToString();
                    
                    if (!HandledGamesCache.Add(uniqueSession, uniqueSession, new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromDays(1) }))
                        goto CONTINUE;
                    
                    var usersGameInfos = new GameUserInfo[playersCount];

                    GameUserInfo currentUserInfo = null;

                    for (int i = 0; i < playersCount; i++)
                    {
                        //var nick = dictionary["player_"+i];
                        var pid = long.Parse(dictionary["PID_" + i]);

                        var info = new GameUserInfo()
                        {
                            Stats = UsersDatabase.Instance.GetStatsDataByProfileId(pid),
                            Race = Enum.Parse<Race>(dictionary["PRace_" + i], true),
                            Team = int.Parse(dictionary["PTeam_" + i]),
                            FinalState = Enum.Parse<PlayerFinalState>(dictionary["PFnlState_" + i]),
                        };
                        
                        usersGameInfos[i] = info;

                        if (pid == state.ProfileId)
                            currentUserInfo = info;
                    }

                    var teams = usersGameInfos.GroupBy(x => x.Team).ToDictionary(x => x.Key, x => x.ToArray());
                    var gameDuration = long.Parse(dictionary["Duration"]);

                    foreach (var team in teams)
                    {
                        for (int i = 0; i < team.Value.Length; i++)
                        {
                            var info = team.Value[i];

                            info.Stats.AllInGameTicks += gameDuration;

                            switch (info.Race)
                            {
                                case Race.space_marine_race:
                                    info.Stats.Smgamescount++;
                                    break;
                                case Race.chaos_marine_race:
                                    info.Stats.Csmgamescount++;
                                    break;
                                case Race.ork_race:
                                    info.Stats.Orkgamescount++;
                                    break;
                                case Race.eldar_race:
                                    info.Stats.Eldargamescount++;
                                    break;
                                case Race.guard_race:
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
                            
                            if (info.FinalState == PlayerFinalState.Winner)
                            {
                                switch (info.Race)
                                {
                                    case Race.space_marine_race:
                                        info.Stats.Smwincount++;
                                        break;
                                    case Race.chaos_marine_race:
                                        info.Stats.Csmwincount++;
                                        break;
                                    case Race.ork_race:
                                        info.Stats.Orkwincount++;
                                        break;
                                    case Race.eldar_race:
                                        info.Stats.Eldarwincount++;
                                        break;
                                    case Race.guard_race:
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
                        }
                    }

                    var chatUserInfo = ChatServer.IrcDaemon.Users[state.ProfileId];
                    var game = chatUserInfo.Game;
                    
                    // For rated games
                    if (game != null && game.Clean())
                    {
                        chatUserInfo.Game = null;

                        var usersInGame = game.UsersInGame;

                        if (usersGameInfos.Select(x => x.Stats.ProfileId).OrderBy(x => x).SequenceEqual(usersInGame.OrderBy(x => x)))
                        {
                            // Update winstreaks for 1v1 only
                            if (usersInGame.Length == 2)
                            {
                                UpdateStreak(usersGameInfos[0]);
                                UpdateStreak(usersGameInfos[1]);
                            }

                            var groupedTeams = usersGameInfos.GroupBy(x => x.Team).Select(x => x.ToArray()).ToArray();
                            
                            var players1Team = groupedTeams[0];
                            var players2Team = groupedTeams[1];

                            Func<StatsData, long> scoreSelector = null;
                            Action<StatsData, long> scoreUpdater = null;

                            switch (usersInGame.Length)
                            {
                                case 2:
                                    scoreSelector = StatsDelegates.Score1v1Selector;
                                    scoreUpdater = StatsDelegates.Score1v1Updated;
                                    break;
                                case 4:
                                    scoreSelector = StatsDelegates.Score2v2Selector;
                                    scoreUpdater = StatsDelegates.Score2v2Updated;
                                    break;
                                case 6:
                                case 8:
                                    scoreSelector = StatsDelegates.Score3v3Selector;
                                    scoreUpdater = StatsDelegates.Score3v3Updated;
                                    break;
                                default: goto UPDATE;
                            }

                            var team0score = (long)players1Team.Average(x => scoreSelector(x.Stats));
                            var team1score = (long)players2Team.Average(x => scoreSelector(x.Stats));

                            var isFirstTeamResult = players1Team.Any(x => x.FinalState == PlayerFinalState.Winner);
                            var delta = EloRating.CalculateELOdelta(team0score, team1score, isFirstTeamResult? EloRating.GameOutcome.Win : EloRating.GameOutcome.Loss);

                            //if (isFirstTeamResult)
                            //{
                                for (int i = 0; i < players1Team.Length; i++)
                                    scoreUpdater(players1Team[i].Stats, Math.Max(1000L, scoreSelector(players1Team[i].Stats) + delta));

                                for (int i = 0; i < players2Team.Length; i++)
                                    scoreUpdater(players2Team[i].Stats, Math.Max(1000L, scoreSelector(players1Team[i].Stats) - delta));
                            /*}
                            else
                            {
                                for (int i = 0; i < players1Team.Length; i++)
                                    scoreUpdater(players1Team[i].Stats, scoreSelector(players1Team[i].Stats) + delta);

                                for (int i = 0; i < players2Team.Length; i++)
                                    scoreUpdater(players1Team[i].Stats, scoreSelector(players1Team[i].Stats) - delta);
                            }*/
                        }
                    }

                    UPDATE:
                    for (int i = 0; i < usersGameInfos.Length; i++)
                        UsersDatabase.Instance.UpdateUserStats(usersGameInfos[i].Stats);
                }
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

        private void UpdateStreak(GameUserInfo info)
        {
            if (info.FinalState == PlayerFinalState.Winner)
            {
                info.Stats.Current1v1Winstreak++;

                if (info.Stats.Current1v1Winstreak > info.Stats.Best1v1Winstreak)
                    info.Stats.Best1v1Winstreak = info.Stats.Current1v1Winstreak;
            }
            else
                info.Stats.Current1v1Winstreak = 0;
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
            public long ProfileId;

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
            public PlayerFinalState FinalState;
        }
    }
}
