namespace SharedServices
{
    public class UserPart
    {
        public string Name { get; set; }
        public ulong SteamId { get; set; }
        public long? ProfileId { get; set; }
        public Race? Race { get; set; }
        public long? Games { get; set; }
        public long? Disconnects { get; set; }
        public long? Average { get; set; }
        public long? Wins { get; set; }
        public long? Score1v1 { get; set; }
        public long? Score2v2 { get; set; }
        public long? Score3v3 { get; set; }
        public long? ScoreBattleRoyale { get; set; }
        public long? Best1v1Winstreak { get; set; }
        public string Status { get; set; }
        public string BFlags { get; set; }
        public string BStats { get; set; }
    }
}
