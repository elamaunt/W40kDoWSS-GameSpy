using Lidgren.Network;
using System;

namespace GSMasterServer.Data
{
    public class PeerState
    {
        public WeakReference<NetConnection> Connection { get; }
        public ulong SteamId { get; }
        public ProfileDBO ActiveProfile { get; set; }
        public string Status { get; set; }
        public string BFlags { get; set; }
        public string BStats { get; set; }

        public PeerState(ulong steamId, NetConnection connection)
        {
            SteamId = steamId;
            Connection = new WeakReference<NetConnection>(connection);
        }
    }
}
