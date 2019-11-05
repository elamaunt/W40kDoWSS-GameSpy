using Framework;
using Lidgren.Network;
using SharedServices;
using System;
using System.Collections.Concurrent;
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
        readonly NetClient _clientPeer;
        ulong _connectedSteamId;

        //NetConnection _connection;
        ServerHailMessage _hailMessage;

        readonly ConcurrentDictionary<string, LoginInfo> _logins = new ConcurrentDictionary<string, LoginInfo>();
        readonly ConcurrentDictionary<ulong, UserInfo> _users = new ConcurrentDictionary<ulong, UserInfo>();
        readonly ConcurrentDictionary<long, StatsInfo> _stats = new ConcurrentDictionary<long, StatsInfo>();

        Timer _reconnectTimer;

        public bool IsConnected { get; private set; }

        public event Action UsersLoaded;
        public event Action<UserInfo> UserDisconnected;
        public event Action<UserInfo> UserConnected;
        public event Action<UserInfo> UserChanged;

        public event Action<GameInfo> GameBroadcastReceived;
        public event Action<MessageInfo> ChatMessageReceived;
        public event Action ConnectionLost;
        public event Action Connected;
        public event Action<LoginInfo> LoginInfoReceived;

        public string ModName => _hailMessage?.ModName;
        public string ModVersion => _hailMessage?.ModVersion;
        public string ActiveGameVariant => _hailMessage?.ActiveGameVariant;

        //public ulong ActiveProfileId { get; set; }
        //public ulong ActiveProfileName { get; set; }

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
                    Logger.Error(ex.Message);
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
            var info = new UserInfo(message.SteamId);

            if (_users.TryAdd(message.SteamId, info))
                UserConnected?.Invoke(info);
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
                    return info = new UserInfo(id);
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
                _users.AddOrUpdate(user.SteamId, id => new UserInfo(id)
                {
                    Name = user.Name,
                    ActiveProfileId = user.ProfileId,
                    Status = user.Status
                }, (id, info) =>
                {
                    info.Name = user.Name;
                    info.ActiveProfileId = user.ProfileId;
                    info.Status = user.Status;

                    return info;
                });

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
                    stats.Name = message.Name;
                    stats.SteamId = message.SteamId;
                    return stats;
                });
            }

            if (_users.TryGetValue(message.SteamId, out UserInfo user))
            {
                user.Name = message.Name;
                user.ActiveProfileId = message.ActiveProfileId;
                user.Best1v1Winstreak = message.Best1v1Winstreak;
                user.Games = message.Games;
                user.Wins = message.Wins;
                user.Score1v1 = message.Score1v1;
                user.Score2v2 = message.Score2v2;
                user.Race = message.Race;

                UserChanged?.Invoke(user);
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
            GameBroadcastReceived?.Invoke(new GameInfo()
            {
                Teamplay = message.Teamplay,
                Ranked = message.Ranked,
                GameVariant = message.GameVariant,
                MaxPlayers = message.MaxPlayers,
                Players = message.Players,
            });
        }

        public void HandleMessage(NetConnection connection, UserStatsChangedMessage message)
        {
            if (_stats.TryGetValue(message.ProfileId, out StatsInfo stats))
            {
                switch (message.GameType)
                {
                    case RatingGameType.Unknown:
                        break;
                    case RatingGameType.Rating1v1:
                        stats.Score1v1 = message.CurrentScore;
                        break;
                    case RatingGameType.Rating2v2:
                        stats.Score2v2 = message.CurrentScore;
                        break;
                    case RatingGameType.Rating3v3_4v4:
                        stats.Score3v3_4v4 = message.CurrentScore;
                        break;
                    default:
                        break;
                }
            }
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

        public void RequestGameBroadcast(bool teamplay, string gameVariant, int maxPlayers, int players, bool ranked)
        {
            var message = _clientPeer.CreateMessage();

            message.WriteJsonMessage(new GameBroadcastMessage()
            {
                Teamplay = teamplay,
                GameVariant = gameVariant,
                MaxPlayers = maxPlayers,
                Players = players,
                Ranked = ranked
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
    }
}
