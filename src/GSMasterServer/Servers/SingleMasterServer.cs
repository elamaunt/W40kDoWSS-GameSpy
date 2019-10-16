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

                        HandleStateConnected(message, _userStates.GetOrAdd(steamId, ep => new PeerState(steamId, message.SenderConnection)));
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
