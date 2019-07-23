using GSMasterServer.Servers;
using Steamworks;
using System.Collections.Concurrent;
using System.Net;

namespace SteamSpy.Utils
{
    public static class PortBindingManager
    {
        static byte[] _receiveBuffer = new byte[1024];
        static Callback<P2PSessionRequest_t> _sessionRequestCallback = Callback<P2PSessionRequest_t>.Create(OnSessionCallbackReceived);
        static Callback<P2PSessionConnectFail_t> _sessionConnectFailedCallback = Callback<P2PSessionConnectFail_t>.Create(OnSessionConnectFailReceived);
        
        static readonly ConcurrentDictionary<CSteamID, ServerRetranslator> PortBindings = new ConcurrentDictionary<CSteamID, ServerRetranslator>();

        private static void OnSessionCallbackReceived(P2PSessionRequest_t param)
        {
            SteamNetworking.AcceptP2PSessionWithUser(param.m_steamIDRemote);
        }

        private static void OnSessionConnectFailReceived(P2PSessionConnectFail_t param)
        {
            //var error = (EP2PSessionError)param.m_eP2PSessionError;

            if (PortBindings.TryRemove(param.m_steamIDRemote, out ServerRetranslator retranslator))
            {
                retranslator.Dispose();
            }
        }

        public static void ClearPortBindings()
        {
            foreach (var item in PortBindings)
                item.Value.Clear();
        }

        public static ushort AddPortBinding(CSteamID id, IPEndPoint localPoint = null)
        {
            return PortBindings.GetOrAdd(id, steamId => new ServerRetranslator(steamId, localPoint)).Port; 
        }

        public static void UpdateFrame()
        {
            uint size, bytesReaded;
            CSteamID remoteSteamId;
            P2PSessionState_t state;


            while (SteamNetworking.IsP2PPacketAvailable(out size))
            {
                if (size == 0)
                    continue;

                if (_receiveBuffer.Length < size)
                    _receiveBuffer = new byte[size];

                if (SteamNetworking.ReadP2PPacket(_receiveBuffer, size, out bytesReaded, out remoteSteamId))
                {
                    if (SteamNetworking.GetP2PSessionState(remoteSteamId, out state))
                    {
                        if (state.m_bConnectionActive == 1)
                        {
                            var retranslator = PortBindings.GetOrAdd(remoteSteamId, steamId => new ServerRetranslator(steamId));

                            retranslator.Send(_receiveBuffer, size);
                        }
                    }
                }
            }
        }
    }
}
