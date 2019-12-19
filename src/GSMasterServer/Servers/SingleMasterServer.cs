extern alias reality;

using GSMasterServer.Data;
using GSMasterServer.Utils;
using IrcNet.Tools;
using Lidgren.Network;
using Newtonsoft.Json;
using reality::Reality.Net.Extensions;
using reality::Reality.Net.GameSpy.Servers;
using SharedServices;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;

namespace GSMasterServer.Servers
{
    public class SingleMasterServer : IClientMessagesHandler
    {
        // Public methods for Discord Bot
        public ProfileDBO[] GetTop => Database.MainDBInstance.Load1v1Top10();
        public int Online => _userStates.Count;


        readonly ConcurrentDictionary<ulong, PeerState> _userStates = new ConcurrentDictionary<ulong, PeerState>();

        readonly NetServer _serverPeer;
        string _lastPlayersTopJson;
        DateTime _lastTopCalculationTime;

        public SingleMasterServer()
        {
            var config = new NetPeerConfiguration("ThunderHawk")
            {
                ConnectionTimeout = 120,
                LocalAddress = IPAddress.Any,
                AutoFlushSendQueue = true,
                AcceptIncomingConnections = true,
                MaximumConnections = 2048,
                Port = 29909,
                PingInterval = 30,
                UseMessageRecycling = true,
                RecycledCacheMaxCount = 128,
                UnreliableSizeBehaviour = NetUnreliableSizeBehaviour.NormalFragmentation
            };

            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.EnableMessageType(NetIncomingMessageType.Data);

            _serverPeer = new NetServer(config);

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());


            _serverPeer.RegisterReceivedCallback(OnSendOrPost, SynchronizationContext.Current);
            _serverPeer.Start();
        }

        void OnSendOrPost(object state)
        {
            //  _serverPeer.
            NetIncomingMessage message = null;
            while (ReadMessage(out message))
            {
                var type = message.MessageType;
                try
                {
                    switch (type)
                    {
                        case NetIncomingMessageType.Data: message.ReadJsonMessage(this); break;
                        case NetIncomingMessageType.ErrorMessage: Logger.Error(message.ReadString()); break;
                        case NetIncomingMessageType.VerboseDebugMessage: Logger.Debug(message.ReadString()); break;
                        case NetIncomingMessageType.WarningMessage: Logger.Warn(message.ReadString()); break;
                        case NetIncomingMessageType.DebugMessage: Logger.Debug(message.ReadString()); break;
                        case NetIncomingMessageType.StatusChanged: HandleStatusChanged(message); break;
                        default: break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.Message);
                }
                finally
                {
                    _serverPeer.Recycle(message);
                    message = null;
                }
            }
        }

        void HandleStatusChanged(NetIncomingMessage message)
        {
            var status = (NetConnectionStatus)message.ReadByte();

            var endPoint = message.SenderEndPoint;

            switch (status)
            {
                case NetConnectionStatus.RespondedAwaitingApproval: HandleConnectionApproval(message); break;
                case NetConnectionStatus.Connected:
                    {
                        var steamId = message.SenderConnection.RemoteHailMessage.PeekUInt64();
                        bool added = false;

                        var lastProfile = Database.MainDBInstance.GetLastActiveProfileForSteamId(steamId);

                        var state = _userStates.GetOrAdd(steamId, ep =>
                        {
                            added = true;
                            var newState = new PeerState(steamId, message.SenderConnection);

                            if (lastProfile.HasValue)
                                newState.ActiveProfile = ProfilesCache.GetProfileByPid(lastProfile.Value.ToString());

                            return newState;
                        });

                        if (state.ActiveProfile == null && lastProfile.HasValue)
                            state.ActiveProfile = ProfilesCache.GetProfileByPid(lastProfile.Value.ToString());

                        state.Connection.SetTarget(message.SenderConnection);

                        if (added)
                            HandleStateConnected(message, state);
                        break;
                    }
                case NetConnectionStatus.Disconnected:
                    {
                        var steamId = message.SenderConnection.RemoteHailMessage.PeekUInt64();

                        if (_userStates.TryRemove(steamId, out PeerState state))
                            HandleStateDisconnected(message, state);
                        break;
                    }
                default:
                    break;
            }
        }

        void HandleConnectionApproval(NetIncomingMessage message)
        {
            try
            {
                var steamId = message.SenderConnection.RemoteHailMessage.PeekUInt64();

                if (steamId == 0)
                {
                    message.SenderConnection.Deny();
                    return;
                }

                var hailMessage = _serverPeer.CreateMessage(new ServerHailMessage()
                {
                    SteamId = steamId,
                    ModName = ServerConstants.ModName,
                    ModVersion = ServerConstants.ModVersion,
                    ActiveGameVariant = ServerConstants.ActiveGameVariant
                }.AsJson());

                message.SenderConnection.Approve(hailMessage);
            }
            catch (Exception)
            {
                message.SenderConnection.Deny();
                throw;
            }
        }

        void HandleStateConnected(NetIncomingMessage message, PeerState state)
        {
            var mes = _serverPeer.CreateMessage();

            mes.WriteJsonMessage(new UserConnectedMessage()
            {
                SteamId = state.SteamId,

                Name = state.ActiveProfile?.Name,
                ActiveProfileId = state.ActiveProfile?.Id,
                Games = state.ActiveProfile?.GamesCount,
                Wins = state.ActiveProfile?.WinsCount,
                Race = state.ActiveProfile?.FavouriteRace,
                Score1v1 = state.ActiveProfile?.Score1v1,
                Score2v2 = state.ActiveProfile?.Score2v2,
                Score3v3 = state.ActiveProfile?.Score3v3,
                Best1v1Winstreak = state.ActiveProfile?.Best1v1Winstreak,
                Average = state.ActiveProfile?.AverageDuractionTicks,
                Disconnects = state.ActiveProfile?.Disconnects,
            });

            _serverPeer.SendToAll(mes, NetDeliveryMethod.ReliableOrdered);
        }

        void HandleStateDisconnected(NetIncomingMessage message, PeerState state)
        {
            if (state.Connection.TryGetTarget(out NetConnection connection))
            {
                if (connection != message.SenderConnection)
                {
                    _userStates[state.SteamId] = state;
                    return;
                }
            }

            var mes = _serverPeer.CreateMessage();

            mes.WriteJsonMessage(new UserDisconnectedMessage()
            {
                SteamId = state.SteamId
            });

            _serverPeer.SendToAll(mes, NetDeliveryMethod.ReliableOrdered);
        }

        bool ReadMessage(out NetIncomingMessage message)
        {
            message = _serverPeer.ReadMessage();
            return message != null;
        }

        //-------------- HANDLERS ---------------

        public void HandleMessage(NetConnection connection, ChatMessageMessage message)
        {
            var mes = _serverPeer.CreateMessage();
            message.LongDate = DateTime.UtcNow.ToBinary();
            mes.WriteJsonMessage(message);
            _serverPeer.SendToAll(mes, NetDeliveryMethod.ReliableUnordered);
        }

        public void HandleMessage(NetConnection connection, UserStatusChangedMessage message)
        {
            var mes = _serverPeer.CreateMessage();
            mes.WriteJsonMessage(message);
            _serverPeer.SendToAll(mes, NetDeliveryMethod.ReliableUnordered);
        }

        public void HandleMessage(NetConnection connection, GameBroadcastMessage message)
        {
            var mes = _serverPeer.CreateMessage();
            message.HostSteamId = connection.RemoteHailMessage.PeekUInt64();
            mes.WriteJsonMessage(message);
            _serverPeer.SendToAll(mes, NetDeliveryMethod.ReliableUnordered);
        }

        public void HandleMessage(NetConnection connection, RequestUserStatsMessage message)
        {
            ProfileDBO profile = null;

            if (message.Name == null)
            {
                if (message.ProfileId != null)
                {
                    profile = ProfilesCache.GetProfileByPid(message.ProfileId.Value.ToString());

                    if (profile == null)
                        return;
                }
            }
            else
            {
                profile = ProfilesCache.GetProfileByName(message.Name);

                if (profile == null)
                    return;
            }

            var statsMessage = new UserStatsMessage()
            {
                ProfileId = profile.Id,
                Name = profile.Name,
                AverageDuration = profile.AverageDuractionTicks,
                Disconnects = profile.Disconnects,
                FavouriteRace = profile.FavouriteRace,
                WinsCount = profile.WinsCount,
                GamesCount = profile.GamesCount,
                Score1v1 = profile.Score1v1,
                Score2v2 = profile.Score2v2,
                Score3v3_4v4 = profile.Score3v3,
                SteamId = profile.SteamId,
                Modified = profile.Modified,
                Best1v1Winstreak = profile.Best1v1Winstreak
            };

            var mes = _serverPeer.CreateMessage();
            mes.WriteJsonMessage(statsMessage);
            _serverPeer.SendMessage(mes, connection, NetDeliveryMethod.ReliableUnordered);
        }

        public void HandleMessage(NetConnection connection, LoginMessage message)
        {
            var steamId = connection.RemoteHailMessage.PeekUInt64();

            var profile = ProfilesCache.GetProfileByName(message.Name);
            var mes = _serverPeer.CreateMessage();

            if (profile == null || !_userStates.TryGetValue(steamId, out PeerState state))
            {
                mes.WriteJsonMessage(new LoginErrorMessage()  
                { 
                    Name = message.Name 
                });

                _serverPeer.SendMessage(mes, connection, NetDeliveryMethod.ReliableOrdered);
                return;
            }

            Database.MainDBInstance.SaveLastActiveProfileForSteamId(steamId, profile.Id);

            state.ActiveProfile = profile;

            // Send login info if needed
            if (message.NeedsInfo)
            {
                mes.WriteJsonMessage(new LoginInfoMessage()
                {
                    Name = profile.Name,
                    Email = profile.Email,
                    PassEnc = profile.Passwordenc,
                    ProfileId = profile.Id
                });

                _serverPeer.SendMessage(mes, connection, NetDeliveryMethod.ReliableOrdered);

                mes = _serverPeer.CreateMessage();
            }

            // Send profile broadcast
            mes.WriteJsonMessage(new UserProfileChangedMessage()
            {
                Name = profile.Name,
                ActiveProfileId = profile.Id,
                Games = profile.GamesCount,
                Wins = profile.WinsCount,
                Race = profile.FavouriteRace,
                Score1v1 = profile.Score1v1,
                Score2v2 = profile.Score2v2,
                Score3v3 = profile.Score3v3,
                Best1v1Winstreak = profile.Best1v1Winstreak,
                Average = profile.AverageDuractionTicks,
                Disconnects = profile.Disconnects,
                SteamId = profile.SteamId
            });

            _serverPeer.SendToAll(mes, NetDeliveryMethod.ReliableOrdered);
        }

        public void HandleMessage(NetConnection connection, LogoutMessage message)
        {
            var steamId = connection.RemoteHailMessage.PeekUInt64();

            if (!_userStates.TryGetValue(steamId, out PeerState state))
                return;

            var mes = _serverPeer.CreateMessage();
            mes.WriteJsonMessage(new UserProfileChangedMessage()
            {
                SteamId = steamId
            });

            _serverPeer.SendToAll(mes, NetDeliveryMethod.ReliableOrdered);
        }
        
        public void HandleMessage(NetConnection connection, GameFinishedMessage message)
        {
            if (message.SessionId == null || message.Players.IsNullOrEmpty())
                return;
            
            message.Date = DateTime.UtcNow;

            var game = new GameDBO();
            game.Id = message.SessionId;
            game.Map = message.Map;
            game.IsRateGame = message.IsRateGame;
            game.UploadedBy = connection.RemoteHailMessage.PeekUInt64();
            game.UploadedDate = DateTime.UtcNow;
            game.Duration = message.Duration;
            game.ModName = message.ModName;
            game.ModVersion = message.ModVersion;
            game.Players = new PlayerData[message.Players.Length];

            var playerInfos = new GamePlayerInfo[message.Players.Length];

            for (int i = 0; i < message.Players.Length; i++)
            {
                var playerPart = message.Players[i];
                var profile = Database.MainDBInstance.GetProfileByName(playerPart.Name);

                if (profile == null)
                    return;


                var player = new PlayerData();

                player.ProfileId = profile.Id;
                player.Race = playerPart.Race;
                player.FinalState = playerPart.FinalState;
                player.Team = (byte)playerPart.Team;

                playerInfos[i] = new GamePlayerInfo() 
                {
                    Data = player,
                    Profile = profile,
                    Part = playerPart,
                    Index = i 
                };

                game.Players[i] = player;
            }

            if (!message.Players.Any(x => x.FinalState == PlayerFinalState.Winner))
                return;

            var teams = playerInfos.GroupBy(x => x.Part.Team).ToDictionary(x => x.Key, x => x.ToArray());

            GameType gameType = GameType.Unknown;

            if (teams.Count == 2)
            {
                if (teams.All(x => x.Value.Length == 1))
                    gameType = GameType._1v1;
                else
                {
                    if (teams.All(x => x.Value.Length == 2))
                        gameType = GameType._2v2;
                    else
                    {
                        if (teams.All(x => x.Value.Length == 3))
                            gameType = GameType._3v3_4v4;
                        else
                        {
                            if (teams.All(x => x.Value.Length == 4))
                                gameType = GameType._3v3_4v4;
                        }
                    }
                }
            }

            for (int i = 0; i < game.Players.Length; i++)
            {
                var info = playerInfos[i];
                info.Profile.AllInGameTicks += message.Duration;
                var playerData = game.Players[i];

                switch (gameType)
                {
                    case GameType.Unknown:
                    case GameType._1v1:
                        playerData.Rating = info.Profile.Score1v1;
                        break;
                    case GameType._2v2:
                        playerData.Rating = info.Profile.Score2v2;
                        break;
                    case GameType._3v3_4v4:
                        playerData.Rating = info.Profile.Score3v3;
                        break;
                    default:
                        break;
                }

                switch (playerData.Race)
                {
                    case Race.space_marine_race:
                        info.Profile.Smgamescount++;
                        break;
                    case Race.chaos_marine_race:
                        info.Profile.Csmgamescount++;
                        break;
                    case Race.ork_race:
                        info.Profile.Orkgamescount++;
                        break;
                    case Race.eldar_race:
                        info.Profile.Eldargamescount++;
                        break;
                    case Race.guard_race:
                        info.Profile.Iggamescount++;
                        break;
                    case Race.necron_race:
                        info.Profile.Necrgamescount++;
                        break;
                    case Race.tau_race:
                        info.Profile.Taugamescount++;
                        break;
                    case Race.dark_eldar_race:
                        info.Profile.Degamescount++;
                        break;
                    case Race.sisters_race:
                        info.Profile.Sobgamescount++;
                        break;
                    default:
                        break;
                }

                if (playerData.FinalState == PlayerFinalState.Winner)
                {
                    switch (playerData.Race)
                    {
                        case Race.space_marine_race:
                            info.Profile.Smwincount++;
                            break;
                        case Race.chaos_marine_race:
                            info.Profile.Csmwincount++;
                            break;
                        case Race.ork_race:
                            info.Profile.Orkwincount++;
                            break;
                        case Race.eldar_race:
                            info.Profile.Eldarwincount++;
                            break;
                        case Race.guard_race:
                            info.Profile.Igwincount++;
                            break;
                        case Race.necron_race:
                            info.Profile.Necrwincount++;
                            break;
                        case Race.tau_race:
                            info.Profile.Tauwincount++;
                            break;
                        case Race.dark_eldar_race:
                            info.Profile.Dewincount++;
                            break;
                        case Race.sisters_race:
                            info.Profile.Sobwincount++;
                            break;
                        default:
                            break;
                    }
                }
            }

            if (message.IsRateGame)
            {
                Func<ProfileDBO, long> scoreSelector = null;
                Action<ProfileDBO, long> scoreUpdater = null;

                switch (gameType)
                {
                    case GameType.Unknown:
                        scoreSelector = StatsDelegates.Score3v3Selector;
                        scoreUpdater = StatsDelegates.Score3v3Updated;
                        break;
                    case GameType._1v1:
                        scoreSelector = StatsDelegates.Score1v1Selector;
                        scoreUpdater = StatsDelegates.Score1v1Updated;
                        break;
                    case GameType._2v2:
                        scoreSelector = StatsDelegates.Score2v2Selector;
                        scoreUpdater = StatsDelegates.Score2v2Updated;
                        break;
                    case GameType._3v3_4v4:
                        scoreSelector = StatsDelegates.Score3v3Selector;
                        scoreUpdater = StatsDelegates.Score3v3Updated;
                        break;
                    default:
                        break;
                }

                var players1Team = teams[0];
                var players2Team = teams[1];

                var team0score = (long)players1Team.Average(x => scoreSelector(x.Profile));
                var team1score = (long)players2Team.Average(x => scoreSelector(x.Profile));

                var isFirstTeamResult = players1Team.Any(x => x.Part.FinalState == PlayerFinalState.Winner);
                var delta = EloRating.CalculateELOdelta(team0score, team1score, isFirstTeamResult ? EloRating.GameOutcome.Win : EloRating.GameOutcome.Loss);

                if (gameType == GameType._1v1)
                {
                    UpdateStreak(playerInfos[0]);
                    UpdateStreak(playerInfos[1]);
                }

                for (int i = 0; i < players1Team.Length; i++)
                {
                    var rx = delta;

                    rx = CorrectDelta(rx, team0score, team1score);

                    var info = players1Team[i];

                    var data = game.Players[info.Index];
                    data.Rating = scoreSelector(info.Profile);
                    data.RatingDelta = rx;

                    var part = message.Players[info.Index];
                    part.Rating = scoreSelector(info.Profile);
                    part.RatingDelta = rx;

                    scoreUpdater(info.Profile, Math.Max(1000L, scoreSelector(info.Profile) + rx));
                }

                for (int i = 0; i < players2Team.Length; i++)
                {
                    var rx = -delta;

                    rx = CorrectDelta(rx, team0score, team1score);

                    var info = players2Team[i];

                    var data = game.Players[info.Index];
                    data.Rating = scoreSelector(info.Profile);
                    data.RatingDelta = rx;

                    var part = message.Players[info.Index];
                    part.Rating = scoreSelector(info.Profile);
                    part.RatingDelta = rx;

                    scoreUpdater(info.Profile, Math.Max(1000L, scoreSelector(info.Profile) + rx));
                }
            }

            message.Type = gameType;
            game.Type = gameType;

            if (Database.MainDBInstance.TryRegisterGame(ref game))
            {
                for (int i = 0; i < playerInfos.Length; i++)
                {
                    var info = playerInfos[i];
                    var profile = info.Profile;
                    Database.MainDBInstance.UpdateProfileData(profile);
                    ProfilesCache.UpdateProfilesCache(profile);
                }

                for (int i = 0; i < playerInfos.Length; i++)
                {
                    var info = playerInfos[i];
                    var profile = info.Profile;

                    var statsMessage = new UserStatsChangedMessage()
                    {
                        ProfileId = profile.Id,
                        AverageDuration = profile.AverageDuractionTicks,
                        CurrentScore = info.Part.Rating + info.Part.RatingDelta,
                        Delta = info.Part.RatingDelta,
                        Disconnects = profile.Disconnects,
                        Games = profile.GamesCount,
                        Wins = profile.WinsCount,
                        Name = profile.Name,
                        GameType = gameType,
                        Race = profile.FavouriteRace,
                        SteamId = profile.SteamId,
                        Winstreak = profile.Best1v1Winstreak
                    };

                    if (_userStates.TryGetValue(profile.SteamId, out PeerState state))
                    {
                        if (state.ActiveProfile?.Id == profile.Id)
                            state.ActiveProfile = profile;
                    }

                    var mess = _serverPeer.CreateMessage();
                    mess.WriteJsonMessage(statsMessage);
                    _serverPeer.SendToAll(mess, NetDeliveryMethod.ReliableOrdered);
                }
                    
                var mes = _serverPeer.CreateMessage();
                mes.WriteJsonMessage(message);
                _serverPeer.SendToAll(mes, NetDeliveryMethod.ReliableOrdered);

                Logger.Trace($"Stats socket: GAME ACCEPTED " + message.SessionId);
                
                Dowstats.UploadGame(playerInfos, game);
            }
            else
            {
                /*for (int i = 0; i < message.Players.Length; i++)
                {
                    var player = message.Players[i];
                    var playerFromDbo = game.Players[i];

                    player.Rating = playerFromDbo.Rating;
                    player.RatingDelta = playerFromDbo.RatingDelta;
                }

                var mes = _serverPeer.CreateMessage();
                mes.WriteJsonMessage(message);
                _serverPeer.SendMessage(mes, connection, NetDeliveryMethod.ReliableOrdered);*/

                Logger.Trace($"Stats socket: GAME REPEATE " + message.SessionId);
            }
        }

        long CorrectDelta(long rx, long team0score, long team1score)
        {
            // Correct rating delta for good players which were beaten by smurfers

            if (rx > 0)
                return rx;

            if (team0score < 1100 || team1score < 1100)
            {
                if (rx < -15)
                    return -15;
            }

            if (team0score < 1200 || team1score < 1200)
            {
                if (rx < -20)
                    return -20;
            }

            if (team0score < 1300 || team1score < 1300)
            {
                if (rx < -25)
                    return -25;
            }

            return rx;
        }

        void UpdateStreak(GamePlayerInfo info)
        {
            if (info.Part.FinalState == PlayerFinalState.Winner)
            {
                info.Profile.Current1v1Winstreak++;

                if (info.Profile.Current1v1Winstreak > info.Profile.Best1v1Winstreak)
                    info.Profile.Best1v1Winstreak = info.Profile.Current1v1Winstreak;
            }
            else
                info.Profile.Current1v1Winstreak = 0;
        }

        public void HandleMessage(NetConnection connection, RequestPlayersTopMessage message)
        {
            if (_lastPlayersTopJson == null || (DateTime.Now - _lastTopCalculationTime).TotalMinutes > 5)
            {
                _lastTopCalculationTime = DateTime.Now;
                var top = Database.MainDBInstance.Load1v1Top10();

                _lastPlayersTopJson = JsonConvert.SerializeObject(new PlayersTopMessage()
                {
                    Stats = top.Select(x => new UserStatsMessage()
                    {
                        ProfileId = x.Id,
                        Name = x.Name,
                        AverageDuration = x.AverageDuractionTicks,
                        Disconnects = x.Disconnects,
                        FavouriteRace = x.FavouriteRace,
                        WinsCount = x.WinsCount,
                        GamesCount = x.GamesCount,
                        Score1v1 = x.Score1v1,
                        Score2v2 = x.Score2v2,
                        Score3v3_4v4 = x.Score3v3,
                        SteamId = x.SteamId,
                        Modified = x.Modified,
                        Best1v1Winstreak = x.Best1v1Winstreak
                    }).ToArray(),
                    Offset = message.Offset,
                    Count = message.Count
                });
            }

            var mes = _serverPeer.CreateMessage();
            mes.WritePlayersTopJsonMessage(_lastPlayersTopJson);
            _serverPeer.SendMessage(mes, connection, NetDeliveryMethod.ReliableOrdered);
        }

        public void HandleMessage(NetConnection connection, RequestUsersMessage message)
        {
            var mes = _serverPeer.CreateMessage();

            mes.WriteJsonMessage(new UsersMessage()
            {
                Users = _userStates.Values.Select(x => new UserPart()
                {
                    Name = x.ActiveProfile?.Name,
                    ProfileId = x.ActiveProfile?.Id,
                    Games = x.ActiveProfile?.GamesCount,
                    Wins = x.ActiveProfile?.WinsCount,
                    Race = x.ActiveProfile?.FavouriteRace,
                    Disconnects = x.ActiveProfile?.Disconnects,
                    Average = x.ActiveProfile?.AverageDuractionTicks,
                    Score1v1 = x.ActiveProfile?.Score1v1,
                    Score2v2 = x.ActiveProfile?.Score2v2,
                    Score3v3 = x.ActiveProfile?.Score3v3,
                    Best1v1Winstreak = x.ActiveProfile?.Best1v1Winstreak,
                    Status = x.Status,
                    BStats = x.BStats,
                    BFlags = x.BFlags,
                    SteamId = x.SteamId
                }).ToArray()
            });

            _serverPeer.SendMessage(mes, connection, NetDeliveryMethod.ReliableOrdered);
        }

        public void HandleMessage(NetConnection connection, RequestLastGamesMessage message)
        {
            var games = Database.MainDBInstance.GetLastGames();

            var mes = _serverPeer.CreateMessage();
            mes.WriteJsonMessage(new LastGamesMessage()
            {
                Games = games.Select(x => new GameFinishedMessage()
                {
                    SessionId = x.Id,
                    Map = x.Map,
                    Type = x.Type,
                    Duration = x.Duration,
                    IsRateGame = x.IsRateGame,
                    ModName = x.ModName,
                    ModVersion = x.ModVersion,
                    Players = x.Players?.Select(p => new PlayerPart()
                    { 
                        FinalState = p.FinalState,
                        Name = ProfilesCache.GetProfileByPid(p.ProfileId.ToString())?.Name,
                        Race = p.Race,
                        Rating = p.Rating,
                        RatingDelta = p.RatingDelta,
                        Team = p.Team
                    }).ToArray()
                }).ToArray()
            });
            _serverPeer.SendMessage(mes, connection, NetDeliveryMethod.ReliableOrdered);
        }

        public void HandleMessage(NetConnection connection, RequestAllUserNicksMessage message)
        {
            var steamId = connection.RemoteHailMessage.PeekUInt64();
            var profiles = Database.MainDBInstance.GetAllProfilesBySteamId(steamId);

            var mes = _serverPeer.CreateMessage();
            mes.WriteJsonMessage(new AllUserNicksMessage()
            {
                Nicks = profiles.Select(x => x.Name).ToArray()
            });

            _serverPeer.SendMessage(mes, connection, NetDeliveryMethod.ReliableOrdered);
        }

        public void HandleMessage(NetConnection connection, RequestNameCheckMessage message)
        {
            var profile = ProfilesCache.GetProfileByName(message.Name);

            var mes = _serverPeer.CreateMessage();
            mes.WriteJsonMessage(new NameCheckMessage()
            {
                Name = message.Name,
                ProfileId = profile?.Id
            });

            _serverPeer.SendMessage(mes, connection, NetDeliveryMethod.ReliableOrdered);
        }

        public void HandleMessage(NetConnection connection, SetKeyValueMessage message)
        {
            var steamId = connection.RemoteHailMessage.PeekUInt64();

            if (_userStates.TryGetValue(steamId, out PeerState state))
            {
                switch (message.Key)
                {
                    case "b_stats":
                        {
                            state.BStats = message.Value;
                            break;
                        }
                    case "b_flags":
                        {
                            state.BFlags = message.Value;
                            break;
                        }
                    default:
                        break;
                }

                message.Name = state.ActiveProfile?.Name;

                var mes = _serverPeer.CreateMessage();
                mes.WriteJsonMessage(message);
                _serverPeer.SendToAll(mes, NetDeliveryMethod.ReliableOrdered);
            }
        }

        public void HandleMessage(NetConnection connection, RequestNewUserMessage message)
        {
            var steamId = connection.RemoteHailMessage.PeekUInt64();

            var nick = message.KeyValues["nick"];
            var email = message.KeyValues["email"];

            var m = new NewUserMessage();

            if (!Database.MainDBInstance.ProfileExists(nick))
            {
                string password = DecryptPassword(message.KeyValues["passwordenc"]);

                var clientData = Database.MainDBInstance.CreateProfile(nick, password.ToMD5(), steamId, email, "??", connection.RemoteEndPoint.Address);

                if (clientData != null)
                {
                    m.Email = email;
                    m.Name = nick;
                    m.Id = clientData.Id;
                }
            }

            var mes = _serverPeer.CreateMessage();
            
            mes.WriteJsonMessage(m);

            _serverPeer.SendMessage(mes, connection, NetDeliveryMethod.ReliableOrdered);
        }

        private static string DecryptPassword(string password)
        {
            string decrypted = gsBase64Decode(password, password.Length);
            gsEncode(ref decrypted);
            return decrypted;
        }
        public static int gsEncode(ref string password)
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
                    num = gsLame(num);
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

        private static int gsLame(int num)
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

        private static string gsBase64Decode(string s, int size)
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
    }
}
