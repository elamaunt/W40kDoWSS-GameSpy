using GSMasterServer.Data;
using System.Collections.Concurrent;

namespace GSMasterServer
{
    public struct LobbyPeerState
    {
        public readonly PeerState User;
        public readonly LobbyState Lobby;
        public readonly ConcurrentDictionary<string, string> KeyValues;
        public readonly long ProfileId;
        public readonly string Nick;

        public LobbyPeerState(PeerState user, LobbyState lobby, ProfileDBO profile)
        {
            User = user;
            Lobby = lobby;
            Nick = profile.Name;
            ProfileId = profile.Id;
            KeyValues = new ConcurrentDictionary<string, string>();
        }
    }
}