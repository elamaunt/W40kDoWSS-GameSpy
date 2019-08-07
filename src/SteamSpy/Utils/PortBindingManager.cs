using GSMasterServer.Servers;
using Steamworks;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace SteamSpy.Utils
{
    public static class PortBindingManager
    {
        static byte[] _receiveBuffer = new byte[1024];
        static Callback<P2PSessionRequest_t> _sessionRequestCallback = Callback<P2PSessionRequest_t>.Create(OnSessionCallbackReceived);
        static Callback<P2PSessionConnectFail_t> _sessionConnectFailedCallback = Callback<P2PSessionConnectFail_t>.Create(OnSessionConnectFailReceived);
        
        static readonly ConcurrentDictionary<CSteamID, ServerSteamPortRetranslator> PortBindings = new ConcurrentDictionary<CSteamID, ServerSteamPortRetranslator>();
        
        private static void OnSessionCallbackReceived(P2PSessionRequest_t param)
        {
            Console.WriteLine($"AcceptP2PSessionWithUser {param.m_steamIDRemote}");
            SteamNetworking.AcceptP2PSessionWithUser(param.m_steamIDRemote);
        }

        private static void OnSessionConnectFailReceived(P2PSessionConnectFail_t param)
        {
            var error = (EP2PSessionError)param.m_eP2PSessionError;

            Console.WriteLine($"OnSessionConnectFailReceived {param.m_steamIDRemote} {error}");

            /*if (PortBindings.TryRemove(param.m_steamIDRemote, out ServerRetranslator retranslator))
            {
                retranslator.Dispose();
            }*/
        }

        public static void ClearPortBindings()
        {
            foreach (var item in PortBindings)
                item.Value.Clear();
        }
        
        public static CSteamID? GetSteamIdByPort(ushort port)
        {
            return PortBindings.Values.FirstOrDefault(x => x.Port == port)?.RemoteUserSteamId;
        }

        public static ServerSteamPortRetranslator AddOrUpdatePortBinding(CSteamID id)
        {
            return PortBindings.GetOrAdd(id, steamId => new ServerSteamPortRetranslator(steamId)); 
        }

        public static void UpdateFrame()
        {
            uint size, bytesReaded;
            CSteamID remoteSteamId;
            P2PSessionState_t state;

            while (SteamNetworking.IsP2PPacketAvailable(out size, 1))
            {
                if (size == 0)
                    continue;

                if (SteamNetworking.ReadP2PPacket(_receiveBuffer, size, out bytesReaded, out remoteSteamId, 1))
                    PortBindings.GetOrAdd(remoteSteamId, steamId => new ServerSteamPortRetranslator(steamId));
            }

            while (SteamNetworking.IsP2PPacketAvailable(out size))
            {
                if (size == 0)
                    continue;

                if (_receiveBuffer.Length < size)
                    _receiveBuffer = new byte[size];
                
                if (SteamNetworking.ReadP2PPacket(_receiveBuffer, size, out bytesReaded, out remoteSteamId))
                {
                    // if (SteamNetworking.GetP2PSessionState(remoteSteamId, out state))
                    // {
                    //     if (state.m_bConnectionActive == 1)
                    //     {
                    var retranslator = PortBindings.GetOrAdd(remoteSteamId, steamId => new ServerSteamPortRetranslator(steamId));

                    retranslator.SendToGame(_receiveBuffer, size);
                    //     }
                    // }
                }
            }
        }
    }
}
