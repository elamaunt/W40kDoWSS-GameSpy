using Framework;
using SharedServices;

namespace ThunderHawk.Core
{
    public class PlayerItemViewModel : ItemViewModel
    {
        public TextFrame Index { get; } = new TextFrame();
        public TextFrame Name { get; } = new TextFrame();
        public TextFrame Rating { get; } = new TextFrame();
        public TextFrame Race { get; } = new TextFrame();
        public TextFrame Winrate { get; } = new TextFrame();
        public TextFrame Games { get; } = new TextFrame();
        public TextFrame Wins { get; } = new TextFrame();
        public TextFrame AverageDuration { get; } = new TextFrame();

        public StatsInfo StatsInfo { get; }
        public PlayerItemViewModel(StatsInfo stats, int index)
        {
            Index.Text = index.ToString();
            StatsInfo = stats;

            Name.Text = stats.Name;
            Rating.Text = stats.Score1v1.ToString();
            Race.Text = GetRaceName(stats.FavouriteRace);
            Games.Text = stats.GamesCount.ToString();
            Wins.Text = stats.WinsCount.ToString();
        }

        string GetRaceName(Race tace)
        {
            switch (tace)
            {
                case SharedServices.Race.space_marine_race: return "SM";
                case SharedServices.Race.chaos_marine_race: return "CSM";
                case SharedServices.Race.ork_race: return "ORK";
                case SharedServices.Race.eldar_race: return "ELD";
                case SharedServices.Race.guard_race: return "IG";
                case SharedServices.Race.necron_race: return "NEC";
                case SharedServices.Race.tau_race: return "TAU";
                case SharedServices.Race.dark_eldar_race: return "DE";
                case SharedServices.Race.sisters_race: return "SOB";
                default: return "Unknown";
            }
        }
    }
}
