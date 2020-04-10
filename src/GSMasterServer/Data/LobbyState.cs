using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GSMasterServer
{
    public class LobbyState
    {
        public readonly ulong HostId;

        public string Indicator;
        public bool Joinable;
        public ReadOnlyDictionary<string, string> Properties;

        public volatile int MaxPlayers;
        public volatile int PlayersCount;
        public bool HostLeftFromThisLobby;

        // Maximum 8 players safety count
        public readonly LinkedList<LobbyPeerState> Members = new LinkedList<LobbyPeerState>();

        public bool GameStarted { get; internal set; }

        public LobbyState(ulong hostId)
        {
            HostId = hostId;
        }
    }
}
