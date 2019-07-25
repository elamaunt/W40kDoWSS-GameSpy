namespace GSMasterServer.Data
{
    public class StatsData
    {
        public object Id;
        public long UserId;
        public long ProfileId;
        public ulong SteamId;
        
        public long Score1v1;
        public long Score2v2;
        public long Score3v3;

        public long Disconnects;
        public long AllInGameTicks;
        public long Current1v1Winstreak;
        public long Best1v1Winstreak;
        public long Modified;

        public long Smgamescount;
        public long Csmgamescount;
        public long Orkgamescount;
        public long Eldargamescount;
        public long Iggamescount;
        public long Necrgamescount;
        public long Taugamescount;
        public long Degamescount;
        public long Sobgamescount;

        public long Smwincount;
        public long Csmwincount;
        public long Orkwincount;
        public long Eldarwincount;
        public long Igwincount;
        public long Necrwincount;
        public long Tauwincount;
        public long Dewincount;
        public long Sobwincount;

        public long GamesCount => Smgamescount + Csmgamescount + Orkgamescount + Eldargamescount + Iggamescount + Necrgamescount + Taugamescount + Degamescount + Sobgamescount;
        public long WinsCount => Smwincount + Csmwincount + Orkwincount + Eldarwincount + Igwincount + Necrwincount + Tauwincount + Dewincount + Sobwincount;

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
    }
}