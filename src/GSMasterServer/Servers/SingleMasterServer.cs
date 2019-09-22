using GSMasterServer.Data;
using IrcNet.Tools;
using Lidgren.Network;
using SharedServices;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;

namespace GSMasterServer.Servers
{
    public class SingleMasterServer
    {
        readonly ConcurrentDictionary<ulong, PeerState> _states = new ConcurrentDictionary<ulong, PeerState>();

        readonly NetServer _serverPeer;

        public SingleMasterServer()
        {
            var config = new NetPeerConfiguration("ThunderHawk")
            {
                ConnectionTimeout = 30,
                LocalAddress = IPAddress.Any,
                AutoFlushSendQueue = true,
                AcceptIncomingConnections = true,
                MaximumConnections = 2048,
                Port = 29909,
                PingInterval = 30000,
                UseMessageRecycling = true,
                RecycledCacheMaxCount = 128,
                UnreliableSizeBehaviour = NetUnreliableSizeBehaviour.NormalFragmentation
            };

            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

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
                        case NetIncomingMessageType.Data: HandleDataMessage(message); break;
                        case NetIncomingMessageType.Error: HandleError(message); break;
                        case NetIncomingMessageType.ErrorMessage: HandleErrorMessage(message); break;
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

                        HandleStateConnected(message, _states.GetOrAdd(steamId, ep => new PeerState(steamId, message.SenderConnection)));
                        break;
                    }
                case NetConnectionStatus.Disconnected:
                    {
                        var steamId = message.SenderConnection.RemoteHailMessage.PeekUInt64();

                        if (_states.TryRemove(steamId, out PeerState state))
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
            _serverPeer.SendToAll(_serverPeer.CreateMessage(  new UserConnectedMessage().AsJson()), state.Connection, NetDeliveryMethod.ReliableUnordered, ConnectedChannel);
        }

        void HandleStateDisconnected(NetIncomingMessage message, PeerState state)
        {
            _serverPeer.SendToAll(_serverPeer.CreateMessage(new UserDisconnectedMessage()
            {
                SteamId = state.SteamId

            }.AsJson()), state.Connection, NetDeliveryMethod.ReliableUnordered, DisconnectedChannel);
        }

        void HandleError(NetIncomingMessage message)
        {
        }

        void HandleErrorMessage(NetIncomingMessage message)
        {
            
        }

        void HandleDataMessage(NetIncomingMessage message)
        {
            /*switch (message.SequenceChannel)
            {
                default:
                    break;
            }*/
        }

        bool ReadMessage(out NetIncomingMessage message)
        {
            message = _serverPeer.ReadMessage();
            return message != null;
        }
    }
}
