using Framework;
using Lidgren.Network;
using SharedServices;
using Steamworks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using ThunderHawk.Core;
using ThunderHawk.Utils;

namespace ThunderHawk
{
    public class SingleMasterServer : IServerMessagesHandler, IMasterServer
    {
        public UserInfo CurrentProfile { get; private set; }

        readonly NetClient _clientPeer;
        ulong _connectedSteamId;

        //NetConnection _connection;
        ServerHailMessage _hailMessage;

        readonly ConcurrentDictionary<string, LoginInfo> _logins = new ConcurrentDictionary<string, LoginInfo>();
        readonly ConcurrentDictionary<ulong, UserInfo> _users = new ConcurrentDictionary<ulong, UserInfo>();
        readonly ConcurrentDictionary<long, StatsInfo> _stats = new ConcurrentDictionary<long, StatsInfo>();

        readonly LinkedList<GameInfo> _lastGames = new LinkedList<GameInfo>();

        public GameInfo[] LastGames => _lastGames.ToArray();

        Timer _reconnectTimer;


        public bool IsConnected { get; private set; }
        public bool IsLastGamesLoaded { get; private set; }
        public bool IsPlayersTopLoaded { get; private set; }

        public event Action UsersLoaded;
        public event Action<UserInfo> UserDisconnected;
        public event Action<string, long?, string> NewUserReceived;
        public event Action<UserInfo, long?, string, string> UserNameChanged;
        public event Action<UserInfo> UserConnected;
        public event Action<UserInfo> UserChanged;
        public event Action<string> LoginErrorReceived;

        public event Action<GameHostInfo> GameBroadcastReceived;
        public event Action<MessageInfo> ChatMessageReceived;
        public event Action ConnectionLost;
        public event Action Connected;
        public event Action<string[]> NicksReceived;
        public event Action<bool> CanAuthorizeReceived;
        public event Action<bool> RegistrationByLauncherReceived;
        public event Action<LoginInfo> LoginInfoReceived;
        public event Action<StatsInfo[], int, int> PlayersTopLoaded;
        public event Action<GameInfo[]> LastGamesLoaded;
        public event Action<GameInfo> NewGameReceived;
        public event Action<string, long?> NameCheckReceived;
        public event Action<string, string, string> UserKeyValueChanged;
        public event Action<StatsChangesInfo> UserStatsChanged;


        StatsInfo[] _currentLoadedLadderTop = new StatsInfo[0];
        readonly Timer _userConnectionCheckTimer;

        public StatsInfo[] PlayersTop => _currentLoadedLadderTop;

        public string ModName => _hailMessage?.ModName;
        public string ModVersion => _hailMessage?.ModVersion;
        public string ActiveGameVariant => _hailMessage?.ActiveGameVariant;

        public SingleMasterServer()
        {
            _clientPeer = new NetClient(new NetPeerConfiguration("ThunderHawk")
            {
                ConnectionTimeout = 60,
                LocalAddress = IPAddress.Any,
                AutoFlushSendQueue = true,
                AcceptIncomingConnections = false,
                MaximumConnections = 2048,
                Port = 0,
                PingInterval = 30,
                UseMessageRecycling = true,
                RecycledCacheMaxCount = 128,
                UnreliableSizeBehaviour = NetUnreliableSizeBehaviour.NormalFragmentation
            });

            _clientPeer.RegisterReceivedCallback(OnSendOrPost, SynchronizationContext.Current);
            _clientPeer.Start();

            SteamUserStates.UserSessionChanged += OnUserSessionChanged;
            _userConnectionCheckTimer = new Timer(OnTimerCallback, null, 2 * 1000, 2 * 1000);
        }

        void OnTimerCallback(object state)
        {
            foreach (var userPair in _users)
            {
                if (userPair.Value.IsUser)
                    continue;

                if (userPair.Value.State != UserState.Connected)
                    SteamUserStates.CheckConnection(userPair.Key);
            }
        }

        void OnUserSessionChanged(ulong steamId, UserState state)
        {
            if (_users.TryGetValue(steamId, out UserInfo info))
            {
                info.State = state;
                UserChanged?.Invoke(info);
            }
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
                        case NetIncomingMessageType.Data: HandleDataMessage(message); break;
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
                    Logger.Error(ex);
                }
                finally
                {
                    _clientPeer.Recycle(message);
                    message = null;
                }
            }
        }

        public void Connect(ulong steamId)
        {
            _reconnectTimer?.Dispose();
            _connectedSteamId = steamId;
            _reconnectTimer = new Timer(Connect, null, 10000, 10000);
            Connect(null);
        }

        void Connect(object state)
        {
            if (IsConnected)
                return;

            var hailMessage = _clientPeer.CreateMessage();
            hailMessage.Write(_connectedSteamId);
            _clientPeer.Connect(GameConstants.SERVER_ADDRESS, 29909, hailMessage);
        }

        public void Disconnect()
        {
            _reconnectTimer?.Dispose();
            if (_connectedSteamId != 0)
                _clientPeer.Disconnect(_connectedSteamId.ToString());
            
        }

        public UserInfo GetUserInfo(ulong steamId)
        {
            return _users.GetOrDefault(steamId);
        }

        public StatsInfo GetStatsInfo(string nick)
        {
            return _stats.FirstOrDefault(x => x.Value.Name == nick).Value;
        }

        public LoginInfo GetLoginInfo(string name)
        {
            return _logins.GetOrDefault(name);
        }

        void HandleStatusChanged(NetIncomingMessage message)
        {
            var status = (NetConnectionStatus)message.ReadByte();

            switch (status)
            {
                case NetConnectionStatus.Connected:
                        HandleStateConnected(message);
                    break;
                case NetConnectionStatus.Disconnected:
                        HandleStateDisconnected(message);
                    break;
                default:
                    break;
            }
        }

        void HandleStateConnected(NetIncomingMessage message)
        {
            _hailMessage = message.SenderConnection.RemoteHailMessage.ReadString().OfJson<ServerHailMessage>();

            RequestUsers();
            IsConnected = true;
            Connected?.Invoke();

            IsLastGamesLoaded = false;
            _lastGames.Clear();
            CoreContext.MasterServer.RequestLastGames();
        }

        public void RequestUsers()
        {
            var message = _clientPeer.CreateMessage();

            message.WriteJsonMessage(new RequestUsersMessage());

            _clientPeer.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
        }
     
        public void RequestLogin(string name)
        {
            var message = _clientPeer.CreateMessage();

            var loginMes = new LoginMessage() { Name = name };

            if (_logins.TryGetValue(name, out LoginInfo info))
            {
                loginMes.NeedsInfo = false;
                LoginInfoReceived?.Invoke(info);
            }
            else
            {
                loginMes.NeedsInfo = true;
            }

            message.WriteJsonMessage(loginMes);

            _clientPeer.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
        }

        public void RequestLogout()
        {
            // TODO
        }

        public void RequestPlayersTop(int offset, int count)
        {
            var message = _clientPeer.CreateMessage();
            message.WriteJsonMessage(new RequestPlayersTopMessage() { Offset = offset, Count = count });
            _clientPeer.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
        }

        public void RequestLastGames()
        {
            var message = _clientPeer.CreateMessage();
            message.WriteJsonMessage(new RequestLastGamesMessage());
            _clientPeer.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
        }

        public StatsInfo[] GetPlayersTop(int offset, int count)
        {
            if (_currentLoadedLadderTop.Length < offset)
                return new StatsInfo[0];

            if (_currentLoadedLadderTop.Length < offset + count)
                count = _currentLoadedLadderTop.Length - offset;

            return new ArraySegment<StatsInfo>(_currentLoadedLadderTop, offset, count).ToArray();
        }

        public void SendGameFinishedInfo(GameFinishedMessage message)
        {
            var mes = _clientPeer.CreateMessage();
            mes.WriteJsonMessage(message);
            _clientPeer.SendMessage(mes, NetDeliveryMethod.ReliableUnordered);
        }

        void HandleStateDisconnected(NetIncomingMessage message)
        {
            IsConnected = false;
            ConnectionLost?.Invoke();
        }

        void HandleDataMessage(NetIncomingMessage message)
        {
            message.ReadJsonMessage(this);
        }

        bool ReadMessage(out NetIncomingMessage message)
        {
            message = _clientPeer.ReadMessage();
            return message != null;
        }

        public void HandleMessage(NetConnection connection, UserDisconnectedMessage message)
        {
            if (_users.TryRemove(message.SteamId, out UserInfo info))
                UserDisconnected?.Invoke(info);
        }

        public void HandleMessage(NetConnection connection, UserConnectedMessage message)
        {
            var info = new UserInfo(message.SteamId, message.SteamId == SteamUser.GetSteamID().m_SteamID);

            info.State = SteamUserStates.GetUserState(message.SteamId);


            info.ActiveProfileId = message.ActiveProfileId;
            info.Average = message.Average;
            info.Best1v1Winstreak = message.Best1v1Winstreak;
            info.Disconnects = message.Disconnects;
            info.Games = message.Games;
            info.Name = message.Name;
            info.Race = message.Race;
            info.Score1v1 = message.Score1v1;
            info.Score2v2 = message.Score2v2;
            info.Score3v3 = message.Score3v3;
            info.Wins = message.Wins;

            if (message.ActiveProfileId.HasValue)
            {
                _stats.AddOrUpdate(message.ActiveProfileId.Value, profileId => new StatsInfo(message.ActiveProfileId.Value)
                {
                    SteamId = message.SteamId,
                    Name = message.Name,
                    FavouriteRace = message.Race.Value,
                    GamesCount = message.Games.Value,
                    WinsCount = message.Wins.Value,
                    Score1v1 = message.Score1v1.Value,
                    Score2v2 = message.Score2v2.Value,
                    Score3v3_4v4 = message.Score3v3.Value,
                    Disconnects = message.Disconnects.Value,
                    AverageDuration = message.Average.Value
                }, (profileId, stats) =>
                {
                    stats.SteamId = message.SteamId;
                    stats.Name = message.Name;
                    stats.FavouriteRace = message.Race.Value;
                    stats.GamesCount = message.Games.Value;
                    stats.WinsCount = message.Wins.Value;
                    stats.Score1v1 = message.Score1v1.Value;
                    stats.Score2v2 = message.Score2v2.Value;
                    stats.Score3v3_4v4 = message.Score3v3.Value;
                    stats.Disconnects = message.Disconnects.Value;
                    stats.AverageDuration = message.Average.Value;

                    return stats;
                });
            }

            if (_users.TryAdd(message.SteamId, info))
                UserConnected?.Invoke(info);

            SteamUserStates.CheckConnection(message.SteamId);
        }

        public void HandleMessage(NetConnection connection, ChatMessageMessage message)
        {
            if (message.SteamId == 0)
                return;

            bool added = false;
            UserInfo info = null;

            ChatMessageReceived?.Invoke(new MessageInfo()
            {
                Author = _users.GetOrAdd(message.SteamId, id =>
                {
                    added = true;
                    return info = new UserInfo(id, id == SteamUser.GetSteamID().m_SteamID);
                }),
                Text = message.Text,
                Date = DateTime.FromBinary(message.LongDate),
                FromGame = message.FromGame
            });

            if (added)
                UserConnected?.Invoke(info);
        }

        public void HandleMessage(NetConnection connection, UsersMessage message)
        {
            var clone = _users.ToDictionary(x => x.Key, x => x.Value);

            // TODO: дописать стату
            for (int i = 0; i < message.Users.Length; i++)
            {
                var user = message.Users[i];
                clone.Remove(user.SteamId);
                var userInfo = _users.AddOrUpdate(user.SteamId, id => new UserInfo(id, id == SteamUser.GetSteamID().m_SteamID)
                {
                    Name = user.Name,
                    ActiveProfileId = user.ProfileId,
                    Status = user.Status,
                    BStats = user.BStats,
                    BFlags = user.BFlags,
                    Score1v1 = user.Score1v1,
                    Score2v2 = user.Score2v2,
                    Score3v3 = user.Score3v3,
                    Wins = user.Wins,
                    Games = user.Games,
                    Average = user.Average,
                    Disconnects = user.Disconnects,
                    Best1v1Winstreak = user.Best1v1Winstreak,
                    Race = user.Race,
                    State = SteamUserStates.GetUserState(user.SteamId)
                }, (id, info) =>
                {
                    info.Name = user.Name;
                    info.ActiveProfileId = user.ProfileId;
                    info.Status = user.Status;
                    info.BStats = user.BStats;
                    info.BFlags = user.BFlags;
                    info.Score1v1 = user.Score1v1;
                    info.Score2v2 = user.Score2v2;
                    info.Score3v3 = user.Score3v3;
                    info.Wins = user.Wins;
                    info.Games = user.Games;
                    info.Average = user.Average;
                    info.Disconnects = user.Disconnects;
                    info.Best1v1Winstreak = user.Best1v1Winstreak;
                    info.Race = user.Race;
                    info.State = SteamUserStates.GetUserState(user.SteamId);

                    return info;
                });

                if (user.SteamId == SteamUser.GetSteamID().m_SteamID)
                    CurrentProfile = userInfo;
                else
                    SteamUserStates.CheckConnection(user.SteamId);

                if (!user.ProfileId.HasValue)
                    continue;

                _stats.AddOrUpdate(user.ProfileId.Value, id => new StatsInfo(id)
                {
                    Score1v1 = user.Score1v1.Value,
                    Score2v2 = user.Score2v2.Value,
                    Score3v3_4v4 = user.Score3v3.Value,
                    WinsCount = user.Wins.Value,
                    GamesCount = user.Games.Value,
                    AverageDuration = user.Average.Value,
                    Disconnects = user.Disconnects.Value,
                    Best1v1Winstreak = user.Best1v1Winstreak.Value,
                    FavouriteRace = user.Race.Value,
                    Name = user.Name,
                    SteamId = user.SteamId
                }, (id, stats) =>
                {
                    stats.Score1v1 = user.Score1v1.Value;
                    stats.Score2v2 = user.Score2v2.Value;
                    stats.Score3v3_4v4 = user.Score3v3.Value;
                    stats.WinsCount = user.Wins.Value;
                    stats.GamesCount = user.Games.Value;
                    stats.AverageDuration = user.Average.Value;
                    stats.Disconnects = user.Disconnects.Value;
                    stats.FavouriteRace = user.Race.Value;
                    stats.Best1v1Winstreak = user.Best1v1Winstreak.Value;
                    stats.Name = user.Name;
                    stats.SteamId = user.SteamId;
                    return stats;
                });
            }

            UsersLoaded?.Invoke();
        }

        public void HandleMessage(NetConnection connection, UserProfileChangedMessage message)
        {
            if (message.ActiveProfileId.HasValue)
            {
                _stats.AddOrUpdate(message.ActiveProfileId.Value, id => new StatsInfo(id)
                {
                    Score1v1 = message.Score1v1.Value,
                    Score2v2 = message.Score2v2.Value,
                    Score3v3_4v4 = message.Score3v3.Value,
                    WinsCount = message.Wins.Value,
                    GamesCount = message.Games.Value,
                    AverageDuration = message.Average.Value,
                    Disconnects = message.Disconnects.Value,
                    Best1v1Winstreak = message.Best1v1Winstreak.Value,
                    FavouriteRace = message.Race.Value,
                    Name = message.Name,
                    SteamId = message.SteamId
                }, (id, stats) =>
                {
                    stats.Score1v1 = message.Score1v1.Value;
                    stats.Score2v2 = message.Score2v2.Value;
                    stats.Score3v3_4v4 = message.Score3v3.Value;
                    stats.WinsCount = message.Wins.Value;
                    stats.GamesCount = message.Games.Value;
                    stats.AverageDuration = message.Average.Value;
                    stats.Disconnects = message.Disconnects.Value;
                    stats.FavouriteRace = message.Race.Value;
                    stats.Best1v1Winstreak = message.Best1v1Winstreak.Value;
                    stats.Name = message.Name;
                    stats.SteamId = message.SteamId;
                    return stats;
                });
            }

            if (_users.TryGetValue(message.SteamId, out UserInfo user))
            {
                var previousName = user.Name;
                var previousProfile = user.ActiveProfileId;

                user.Name = message.Name;

                user.ActiveProfileId = message.ActiveProfileId;

                user.Best1v1Winstreak = message.Best1v1Winstreak;
                user.Games = message.Games;
                user.Wins = message.Wins;
                user.Score1v1 = message.Score1v1;
                user.Score2v2 = message.Score2v2;
                user.Score3v3 = message.Score3v3;
                user.Race = message.Race;

                UserChanged?.Invoke(user);
                UserNameChanged?.Invoke(user, previousProfile, previousName, user.Name);
            }
        }

        public void HandleMessage(NetConnection connection, UserStatusChangedMessage message)
        {
            if (_users.TryGetValue(message.SteamId, out UserInfo user))
            {
                user.Status = message.Status;
                UserChanged?.Invoke(user);
            }
        }

        public void HandleMessage(NetConnection connection, GameBroadcastMessage message)
        {
            GameBroadcastReceived?.Invoke(new GameHostInfo()
            {
                Teamplay = message.Teamplay,
                Ranked = message.Ranked,
                GameVariant = message.GameVariant,
                MaxPlayers = message.MaxPlayers,
                Players = message.Players,
                Score = message.Score,
                LimitedByRating = message.LimitedByRating,
                IsUser = message.HostSteamId == SteamUser.GetSteamID().m_SteamID
            });
        }

        public void HandleMessage(NetConnection connection, UserStatsChangedMessage message)
        {
            var changesInfo = new StatsChangesInfo()
            {
                Name = message.Name,
                CurrentScore = message.CurrentScore,
                Delta = message.Delta,
                GameType = message.GameType
            };

            if (_users.TryGetValue(message.SteamId, out UserInfo info))
            {
                if (message.ProfileId == info.ActiveProfileId)
                {
                    info.Games = message.Games;
                    info.Wins = message.Wins;
                    info.Best1v1Winstreak = message.Winstreak;
                    info.Race = message.Race;
                    info.Average = message.AverageDuration;
                    info.Disconnects = message.Disconnects;

                    if (message.CurrentScore != 0)

                        switch (message.GameType)
                        {
                            case GameType.Unknown:
                                break;
                            case GameType._1v1:
                                info.Score1v1 = Math.Max(1000, message.CurrentScore);
                                break;
                            case GameType._2v2:
                                info.Score2v2 = Math.Max(1000, message.CurrentScore);
                                break;
                            case GameType._3v3_4v4:
                                info.Score3v3 = Math.Max(1000, message.CurrentScore);
                                break;
                            default:
                                break;
                        }

                    changesInfo.User = info;
                }
            }

            if (_stats.TryGetValue(message.ProfileId, out StatsInfo stats))
            {
                stats.GamesCount = message.Games;
                stats.WinsCount = message.Wins;
                stats.Best1v1Winstreak = message.Winstreak;
                stats.FavouriteRace = message.Race;
                stats.AverageDuration = message.AverageDuration;
                stats.Disconnects = message.Disconnects;
                
                if (message.CurrentScore != 0)
                    switch (message.GameType)
                    {
                        case GameType.Unknown:
                            break;
                        case GameType._1v1:
                            stats.Score1v1 = Math.Max(1000, message.CurrentScore);
                            break;
                        case GameType._2v2:
                            stats.Score2v2 = Math.Max(1000, message.CurrentScore);
                            break;
                        case GameType._3v3_4v4:
                            stats.Score3v3_4v4 = Math.Max(1000, message.CurrentScore);
                            break;
                        default:
                            break;
                    }
            }

            UserStatsChanged?.Invoke(changesInfo);
        }

        public void HandleMessage(NetConnection connection, UserStatsMessage message)
        {
            _stats.AddOrUpdate(message.ProfileId, profileId => new StatsInfo(message.ProfileId)
            {
                SteamId = message.SteamId,
                Name = message.Name,
                FavouriteRace = message.FavouriteRace,
                GamesCount = message.GamesCount,
                WinsCount = message.WinsCount,
                Score1v1 = message.Score1v1,
                Score2v2 = message.Score2v2,
                Score3v3_4v4 = message.Score3v3_4v4,
                Disconnects = message.Disconnects,
                AverageDuration = message.AverageDuration
            }, (profileId, stats) =>
            {
                stats.SteamId = message.SteamId;
                stats.Name = message.Name;
                stats.FavouriteRace = message.FavouriteRace;
                stats.GamesCount = message.GamesCount;
                stats.WinsCount = message.WinsCount;
                stats.Score1v1 = message.Score1v1;
                stats.Score2v2 = message.Score2v2;
                stats.Score3v3_4v4 = message.Score3v3_4v4;
                stats.Disconnects = message.Disconnects;
                stats.AverageDuration = message.AverageDuration;

                return stats;
            });
        }

        public UserInfo[] GetAllUsers()
        {
            return _users.Values.ToArray();
        }

        public StatsInfo GetStatsInfo(long profileId)
        {
            return _stats.GetOrAdd(profileId, id =>
            {
                RequestUserStats(id);
                return new StatsInfo(id);
            });
        }

        public void RequestNewUser(Dictionary<string, string> pairs)
        {
            CoreContext.OpenLogsService.Log($"RequestNewUser");

            var message = _clientPeer.CreateMessage();

            message.WriteJsonMessage(new RequestNewUserMessage()
            {
                KeyValues = pairs
            });

            _clientPeer.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
        }

        public void RequestAllUserNicks(string email)
        {
            var message = _clientPeer.CreateMessage();

            message.WriteJsonMessage(new RequestAllUserNicksMessage()
            {
                Email = email
            });

           _clientPeer.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
        }
        
        public void RequestRegistration(string login, string password)
        {
            var message = _clientPeer.CreateMessage();

            message.WriteJsonMessage(new RequestRegistrationByLauncher
            {
                Login = login,
                Password = password
            });

            _clientPeer.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
        }

        public void RequestCanAuthorize(string login, string password)
        {
            var message = _clientPeer.CreateMessage();

            message.WriteJsonMessage(new RequestCanAuthorizeMessage
            {
                Login = login,
                Password = password
            });

            _clientPeer.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
        }

        public void RequestNameCheck(string name)
        {
            var message = _clientPeer.CreateMessage();

            message.WriteJsonMessage(new RequestNameCheckMessage()
            {
                Name = name
            });

            _clientPeer.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
        }


        public void RequestGameBroadcast(bool teamplay, string gameVariant, int maxPlayers, int players, int score, bool limitedByRating, bool ranked)
        {
            var message = _clientPeer.CreateMessage();

            message.WriteJsonMessage(new GameBroadcastMessage()
            {
                Teamplay = teamplay,
                GameVariant = gameVariant,
                MaxPlayers = maxPlayers,
                Players = players,
                Ranked = ranked,
                LimitedByRating = limitedByRating,
                Score = score
            });

            _clientPeer.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
        }

        public void RequestUserStats(long profileId)
        {
            var message = _clientPeer.CreateMessage();

            message.WriteJsonMessage(new RequestUserStatsMessage()
            {
                ProfileId = profileId
            });

            _clientPeer.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
        }

        public void RequestUserStats(string name)
        {
            var message = _clientPeer.CreateMessage();

            message.WriteJsonMessage(new RequestUserStatsMessage()
            {
                Name = name
            });

            _clientPeer.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
        }

        public void SendChatMessage(string text, bool fromGame)
        {
            var message = _clientPeer.CreateMessage();

            message.WriteJsonMessage(new ChatMessageMessage()
            {
                SteamId = _connectedSteamId,
                Text = text,
                FromGame = fromGame
            });

            _clientPeer.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
        }

        public void HandleMessage(NetConnection connection, NewProfileMessage message)
        {
            if (!_users.TryGetValue(message.SteamId, out UserInfo info))
                return;

            info.ActiveProfileId = message.Id;
            info.Name = message.Name;

            UserChanged?.Invoke(info);
        }

        public void HandleMessage(NetConnection connection, RegisterErrorMessage message)
        {
            Debugger.Break();
        }

        public void HandleMessage(NetConnection connection, LoginInfoMessage message)
        {
            var info = new LoginInfo()
            {
                Name = message.Name,
                Email = message.Email,
                PassEnc = message.PassEnc,
                ProfileId = message.ProfileId
            };

            _logins[message.Name] = info;

            LoginInfoReceived?.Invoke(info);
        }

        public void HandleMessage(NetConnection connection, PlayersTopMessage message)
        {
            if (message.Offset + message.Count > _currentLoadedLadderTop.Length)
                Array.Resize(ref _currentLoadedLadderTop, message.Offset + message.Count);

            for (int i = 0; i < message.Count; i++)
            {
                if (i < message.Stats.Length)
                    _currentLoadedLadderTop[i + message.Offset] = ToStatsInfo(message.Stats[i]);
                else
                    _currentLoadedLadderTop[i + message.Offset] = null;
            }

            IsPlayersTopLoaded = true;
            PlayersTopLoaded?.Invoke(_currentLoadedLadderTop, message.Offset, message.Count);
        }

        StatsInfo ToStatsInfo(UserStatsMessage message)
        {
            var stats = new StatsInfo(message.ProfileId)
            {
                SteamId = message.SteamId,
                Name = message.Name,
                Modified = message.Modified,
                AverageDuration = message.AverageDuration,
                Disconnects = message.Disconnects,
                Best1v1Winstreak = message.Best1v1Winstreak,
                FavouriteRace = message.FavouriteRace,
                GamesCount = message.GamesCount,
                WinsCount = message.WinsCount,
                Score1v1 = message.Score1v1,
                Score2v2 = message.Score2v2,
                Score3v3_4v4 = message.Score3v3_4v4
            };

            return _stats[message.ProfileId] = stats;
        }

        public void HandleMessage(NetConnection senderConnection, LastGamesMessage lastGamesMessage)
        {
            for (int i = lastGamesMessage.Games.Length - 1; i >= 0; i--)
            {
                var game = lastGamesMessage.Games[i];

                _lastGames.AddFirst(new GameInfo()
                {
                    SessionId = game.SessionId,
                    Map = game.Map,
                    ModName = game.ModName,
                    ModVersion = game.ModVersion,
                    IsRateGame = game.IsRateGame,
                    Type = game.Type,
                    Duration = game.Duration,
                    PlayedDate = game.Date,

                    Players = game.Players.Select(x => new PlayerInfo()
                    {
                        Name  = x.Name,
                        Team = x.Team,
                        Race = x.Race,
                        FinalState = x.FinalState,
                        Rating = x.Rating,
                        RatingDelta = x.RatingDelta
                    }).ToArray()
                });
            }

            IsLastGamesLoaded = true;
            LastGamesLoaded?.Invoke(_lastGames.ToArray());
        }

        public void HandleMessage(NetConnection senderConnection, AllUserNicksMessage message)
        {
            NicksReceived?.Invoke(message.Nicks ?? new string[0]);
        }

        public void HandleMessage(NetConnection senderConnection, ResponseCanAuthorizeMessage message)
        {
            CanAuthorizeReceived?.Invoke(message.CanAuthorize);
        }

        public void HandleMessage(NetConnection senderConnection, ResponseRegistrationByLauncherMessage message)
        {
            RegistrationByLauncherReceived?.Invoke(message.RegistrationSuccess);
        }

        public void HandleMessage(NetConnection senderConnection, NameCheckMessage message)
        {
            NameCheckReceived?.Invoke(message.Name, message.ProfileId);
        }

        public void HandleMessage(NetConnection senderConnection, LoginErrorMessage message)
        {
            LoginErrorReceived?.Invoke(message.Name);
        }

        public void SendKeyValuesChanged(string name, string[] pairs)
        {
            if (pairs.Length < 2)
                return;

            for (int i = 0; i < pairs.Length; i+=2)
            {
                var message = _clientPeer.CreateMessage();

                message.WriteJsonMessage(new SetKeyValueMessage()
                {
                    SteamId = SteamUser.GetSteamID().m_SteamID,
                    Name = name,
                    Key = pairs[i],
                    Value = pairs[i + 1]
                });

                _clientPeer.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
            }
        }

        public void HandleMessage(NetConnection connection, SetKeyValueMessage message)
        {
            if (_users.TryGetValue(message.SteamId, out UserInfo info))
            {
                switch (message.Key)
                {
                    case "b_stats":
                        {
                            info.BStats = message.Value;
                            break;
                        }
                    case "b_flags":
                        {
                            info.BFlags = message.Value;
                            break;
                        }
                    default:
                        break;
                }

                UserKeyValueChanged?.Invoke(message.Name, message.Key, message.Value);
                UserChanged?.Invoke(info);
            }
        }

        public void HandleMessage(NetConnection connection, NewUserMessage message)
        {
            NewUserReceived?.Invoke(message.Name, message.Id, message.Email);
        }

        public void HandleMessage(NetConnection connection, GameFinishedMessage message)
        {
            var game = new GameInfo()
            {
                SessionId = message.SessionId,
                IsRateGame = message.IsRateGame,
                Map = message.Map,
                ModName = message.ModName,
                Duration = message.Duration,
                PlayedDate = message.Date,
                Url = message.Url,
                ModVersion = message.ModVersion,
                Type = message.Type,
                Players = message.Players?.Select(x => new PlayerInfo()
                {
                    Name = x.Name,
                    FinalState = x.FinalState,
                    Race = x.Race,
                    Rating = x.Rating,
                    RatingDelta = x.RatingDelta,
                    Team = x.Team
                }).ToArray()
            };

            if (IsLastGamesLoaded)
                _lastGames.AddFirst(game);
            NewGameReceived?.Invoke(game);
        }
    }
}
