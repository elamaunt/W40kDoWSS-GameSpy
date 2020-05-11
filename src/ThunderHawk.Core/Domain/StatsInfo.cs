using SharedServices;

namespace ThunderHawk.Core
{
    public class StatsInfo
    {
        public long ProfileId { get; }

        public ulong SteamId { get; set; }
        public string Name { get; set; }
        public Race FavouriteRace { get; set; }
        public long GamesCount { get; set; }
        public long WinsCount { get; set; }
        public long Score1v1 { get; set; }
        public long Score2v2 { get; set; }
        public long Score3v3_4v4 { get; set; }
        public long ScoreBattleRoyale { get; set; }
        public long AverageDuration { get; set; }
        public long Disconnects { get; set; }
        public long Best1v1Winstreak { get; set; }
        public long Modified { get; set; }

        public StatsInfo(long profileId)
        {
            ProfileId = profileId;
        }

        public float WinRate
        {
            get
            {
                if (GamesCount == 0)
                    return 0f;
                return ((float)WinsCount) / GamesCount;
            }
        }

        public int StarsCount
        {
            get
            {
                if (WinsCount > 150 && WinRate > 0.85f)
                    return 5;

                if (WinsCount > 100 && WinRate > 0.65f)
                    return 4;

                if (WinsCount > 50 && WinRate > 0.5f)
                    return 3;

                if (WinsCount > 25 && WinRate > 0.4f)
                    return 2;

                if (WinsCount > 10)
                    return 1;

                return 0;
            }
        }
    }
}
