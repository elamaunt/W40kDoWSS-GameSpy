using Lidgren.Network;
using System;

namespace GSMasterServer.Data
{
    public class PeerState
    {
        public readonly WeakReference<NetConnection> Connection;
        public readonly ulong SteamId;
        public ProfileDBO ActiveProfile;

        public string Status;
        public string BFlags;
        public string BStats;

        public LobbyPeerState? LobbyState;

        public PeerState(ulong steamId, NetConnection connection)
        {
            SteamId = steamId;
            Connection = new WeakReference<NetConnection>(connection);
        }
    }
}
