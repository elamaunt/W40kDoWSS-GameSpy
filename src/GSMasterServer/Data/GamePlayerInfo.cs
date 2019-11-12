using GSMasterServer.Data;
using SharedServices;

namespace GSMasterServer
{
    public class GamePlayerInfo
    {
        public int Index { get; set; }
        public ProfileDBO Profile { get; set; }
        public PlayerPart Part { get; set; }
        public PlayerData Data { get; set; }
    }
}
