using SharedServices;

namespace ThunderHawk.Core
{
    public class StatsChangesInfo
    {
        public UserInfo User { get; set; }

        public string Name { get; set; }
        public long Delta { get; set; }
        public long CurrentScore { get; set; }
        public GameType GameType { get; set; }
    }
}
