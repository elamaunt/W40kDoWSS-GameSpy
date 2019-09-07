using System.Collections.Generic;

namespace GSMasterServer.Data
{
    public class ProfileDBO
    {
        public long Id { get; set; }
        public ulong SteamId { get; set; }
        
        public string Name { get; set; }
        public string Passwordenc { get; set; }
        public string Email { get; set; }
        public string Country { get; set; }
        public long Session { get; set; }
        public string LastIp { get; set; }

        public List<long> Friends { get; set; }


        public long Score1v1 { get; set; }
        public long Score2v2 { get; set; }
        public long Score3v3 { get; set; }

        public long Disconnects { get; set; }
        public long AllInGameTicks { get; set; }
        public long Current1v1Winstreak { get; set; }
        public long Best1v1Winstreak { get; set; }
        public long Modified { get; set; }

        public long Smgamescount { get; set; }
        public long Csmgamescount { get; set; }
        public long Orkgamescount { get; set; }
        public long Eldargamescount { get; set; }
        public long Iggamescount { get; set; }
        public long Necrgamescount { get; set; }
        public long Taugamescount { get; set; }
        public long Degamescount { get; set; }
        public long Sobgamescount { get; set; }

        public long Smwincount { get; set; }
        public long Csmwincount { get; set; }
        public long Orkwincount { get; set; }
        public long Eldarwincount { get; set; }
        public long Igwincount { get; set; }
        public long Necrwincount { get; set; }
        public long Tauwincount { get; set; }
        public long Dewincount { get; set; }
        public long Sobwincount { get; set; }

        public long GamesCount => Smgamescount + Csmgamescount + Orkgamescount + Eldargamescount + Iggamescount + Necrgamescount + Taugamescount + Degamescount + Sobgamescount;
        public long WinsCount => Smwincount + Csmwincount + Orkwincount + Eldarwincount + Igwincount + Necrwincount + Tauwincount + Dewincount + Sobwincount;

        public float WinRate
        {
            get
            {
                if (GamesCount == 0)
                    return 0f;
                return ((float)WinsCount) / GamesCount;
            }
        }

        public long AverageDuractionTicks
        {
            get
            {
                if (GamesCount == 0)
                    return 0;
                return AllInGameTicks / GamesCount;
            }
        }

        public Race FavouriteRace
        {
            get
            {
                Race race = Race.random;
                long count = 0L;

                if (Smgamescount > count)
                {
                    count = Smgamescount;
                    race = Race.space_marine_race;
                }

                if (Csmgamescount > count)
                {
                    count = Csmgamescount;
                    race = Race.chaos_marine_race;
                }

                if (Orkgamescount > count)
                {
                    count = Orkgamescount;
                    race = Race.ork_race;
                }

                if (Eldargamescount > count)
                {
                    count = Eldargamescount;
                    race = Race.eldar_race;
                }

                if (Iggamescount > count)
                {
                    count = Iggamescount;
                    race = Race.guard_race;
                }

                if (Necrgamescount > count)
                {
                    count = Necrgamescount;
                    race = Race.necron_race;
                }

                if (Taugamescount > count)
                {
                    count = Taugamescount;
                    race = Race.tau_race;
                }

                if (Degamescount > count)
                {
                    count = Degamescount;
                    race = Race.dark_eldar_race;
                }

                if (Sobgamescount > count)
                {
                    count = Sobgamescount;
                    race = Race.sisters_race;
                }

                return race;
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