using GSMasterServer.Servers;
using Steamworks;
using System.Collections.Concurrent;
using System.Linq;

namespace ThunderHawk.Utils
{
    public static class PortBindingManager
    {
        static byte[] _receiveBuffer = new byte[1024];
    
        static readonly ConcurrentDictionary<CSteamID, ServerSteamPortRetranslator> PortBindings = new ConcurrentDictionary<CSteamID, ServerSteamPortRetranslator>();

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
