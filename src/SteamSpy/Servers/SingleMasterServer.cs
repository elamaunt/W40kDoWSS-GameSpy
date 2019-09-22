using Lidgren.Network;
using SharedServices;
using Steamworks;
using System;
using System.Net;
using System.Threading;
using ThunderHawk.Core;
using ThunderHawk.Utils;
using Framework;

namespace ThunderHawk
{
    public class SingleMasterServer
    {
        readonly NetClient _clientPeer;

        NetConnection _connection;
        ServerHailMessage _hailMessage;

        public SingleMasterServer(IPAddress address, int port)
        {
            _clientPeer = new NetClient(new NetPeerConfiguration("ThunderHawk")
            {
                ConnectionTimeout = 30,
                LocalAddress = address,
                AutoFlushSendQueue = true,
                AcceptIncomingConnections = false,
                MaximumConnections = 2048,
                Port = port,
                PingInterval = 30000,
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

        public void Connect(CSteamID steamId)
        {
            var hailMessage = _clientPeer.CreateMessage();
            hailMessage.Write(steamId.m_SteamID);

            _connection = _clientPeer.Connect(GameConstants.SERVER_ADDRESS, 29909, hailMessage);
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
        }

        void HandleStateDisconnected(NetIncomingMessage message)
        {
            _hailMessage = null;
            _connection = null;
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
            message = _clientPeer.ReadMessage();
            return message != null;
        }

    }
}
