using Lidgren.Network;

namespace GSMasterServer.Data
{
    public class PeerState
    {
        public NetConnection Connection { get; }
        public ulong SteamId { get; }
        public ProfileDBO ActiveProfile { get; set; }
        public string Status { get;  set; }

        public PeerState(ulong steamId, NetConnection connection)
        {
            SteamId = steamId;
            Connection = connection;
        }
    }
}
