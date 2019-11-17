using Framework;

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
            Race.Text = stats.FavouriteRace.ToString();
            Games.Text = stats.GamesCount.ToString();
            Wins.Text = stats.WinsCount.ToString();
        }
    }
}
