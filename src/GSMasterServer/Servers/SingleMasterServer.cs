using GSMasterServer.Data;
using GSMasterServer.Utils;
using IrcNet.Tools;
using Lidgren.Network;
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
        readonly ConcurrentDictionary<ulong, PeerState> _userStates = new ConcurrentDictionary<ulong, PeerState>();

        readonly NetServer _serverPeer;

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

                        var state = _userStates.GetOrAdd(steamId, ep =>
                        {
                            added = true;
                            return new PeerState(steamId, message.SenderConnection);
                        });

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
                SteamId = state.SteamId
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
                SteamId = profile.SteamId
            };

            var mes = _serverPeer.CreateMessage();
            mes.WriteJsonMessage(statsMessage);
            _serverPeer.SendMessage(mes, connection, NetDeliveryMethod.ReliableUnordered);
        }

        public void HandleMessage(NetConnection connection, LoginMessage message)
        {
            var steamId = connection.RemoteHailMessage.PeekUInt64();

            var profile = Database.MainDBInstance.GetProfilesBySteamId((long)steamId).FirstOrDefault(x => StringComparer.InvariantCultureIgnoreCase.Compare(x.Name, message.Name) == 0);

            if (profile == null)
                return;

            if (!_userStates.TryGetValue(steamId, out PeerState state))
                return;

            state.ActiveProfile = profile;

            var mes = _serverPeer.CreateMessage();

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

            state.ActiveProfile = null;

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

            var game = new GameDBO();
            game.Id = message.SessionId;
            game.IsRateGame = message.IsRateGame;
            game.UploadedBy = connection.RemoteHailMessage.PeekUInt64();
            game.Date = DateTime.UtcNow;
            game.Duration = message.Duration;
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
                }

                var mes = _serverPeer.CreateMessage();
                mes.WriteJsonMessage(message);
                _serverPeer.SendToAll(mes, NetDeliveryMethod.ReliableOrdered);

                Logger.Trace($"Stats socket: GAME ACCEPTED " + message.SessionId);
                //Dowstats.UploadGame(dictionary, usersGameInfos, isRateGame);
            }
            else
            {
                for (int i = 0; i < message.Players.Length; i++)
                {
                    var player = message.Players[i];
                    var playerFromDbo = game.Players[i];

                    player.Rating = playerFromDbo.Rating;
                    player.RatingDelta = playerFromDbo.RatingDelta;
                }

                var mes = _serverPeer.CreateMessage();
                mes.WriteJsonMessage(message);
                _serverPeer.SendMessage(mes, connection, NetDeliveryMethod.ReliableOrdered);
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
            // TODO
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
                    SteamId = x.SteamId
                }).ToArray()
            });

            _serverPeer.SendMessage(mes, connection, NetDeliveryMethod.ReliableOrdered);
        }
    }
}
