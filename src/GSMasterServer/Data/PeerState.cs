using Lidgren.Network;

namespace GSMasterServer.Data
{
    public class PeerState
    {
        public NetConnection Connection { get; }
        public ulong SteamId { get; }

        public PeerState(ulong steamId, NetConnection connection)
        {
            SteamId = steamId;
            Connection = connection;
        }
    }
}
