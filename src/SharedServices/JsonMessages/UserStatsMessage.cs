﻿namespace SharedServices
{
    public class UserStatsMessage
    {
        public ulong SteamId { get; set; }
        public long ProfileId { get; set; }
        public string Name { get; set; }
        public Race FavouriteRace { get; set; }
        public long GamesCount { get; set; }
        public long WinsCount { get; set; }
        public long Score1v1 { get; set; }
        public long Score2v2 { get; set; }
        public long Score3v3_4v4 { get; set; }
        public long AverageDuration { get; set; }
        public long Disconnects { get; set; }
    }
}
