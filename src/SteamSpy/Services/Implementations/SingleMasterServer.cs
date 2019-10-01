using Framework;
using Lidgren.Network;
using SharedServices;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using ThunderHawk.Core;
using ThunderHawk.Utils;

namespace ThunderHawk
{
    public class SingleMasterServer : IMessagesHandler, IMasterServer
    {
        readonly NetClient _clientPeer;
        ulong _connectedSteamId;

        //NetConnection _connection;
        ServerHailMessage _hailMessage;

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
            });

            if (added)
                UserConnected?.Invoke(info);
        }

        public void HandleMessage(NetConnection connection, UsersMessage message)
        {
            var clone = _users.ToDictionary(x => x.Key, x => x.Value);

            for (int i = 0; i < message.Users.Length; i++)
            {
                var user = message.Users[i];
                clone.Remove(user.SteamId);
                _users.AddOrUpdate(user.SteamId, id => new UserInfo(id), (id, info) =>
                {
                    info.Name = user.Name;
                    info.ActiveProfileId = user.ProfileId;
                    info.Status = user.Status;

                    return info;
                });
            }

            UsersLoaded?.Invoke();
        }

        public void HandleMessage(NetConnection connection, UserNameChangedMessage message)
        {
            if (_users.TryGetValue(message.SteamId, out UserInfo user))
            {
                user.Name = message.NewName;
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

        public void HandleMessage(NetConnection connection, RequestUserStatsMessage message)
        {
            // Nothing
        }

        public void HandleMessage(NetConnection connection, LoginMessage message)
        {
            // Nothing
        }

        public void HandleMessage(NetConnection connection, LogoutMessage message)
        {
            // Nothing
        }

        public void HandleMessage(NetConnection connection, GameFinishedMessage message)
        {
            // Nothing
        }
        public void HandleMessage(NetConnection connection, RequestUsersMessage message)
        {
            // Nothing
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

        public void SendChatMessage(string text)
        {
            var message = _clientPeer.CreateMessage();

            message.WriteJsonMessage(new ChatMessageMessage()
            {
                SteamId = _connectedSteamId,
                Text = text
            });

            _clientPeer.SendMessage(message, NetDeliveryMethod.ReliableUnordered);
        }
    }
}
