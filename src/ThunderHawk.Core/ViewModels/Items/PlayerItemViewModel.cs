using Framework;

namespace ThunderHawk.Core
{
    public class PlayerItemViewModel : ItemViewModel
    {
        public TextFrame Name { get; } = new TextFrame();
        public TextFrame Rating { get; } = new TextFrame();
        public TextFrame Race { get; } = new TextFrame();
        public TextFrame Winrate { get; } = new TextFrame();
        public TextFrame Games { get; } = new TextFrame();
        public TextFrame Wins { get; } = new TextFrame();
        public TextFrame AverageDuration { get; } = new TextFrame();

        public StatsInfo StatsInfo { get; }
        public PlayerItemViewModel(StatsInfo statsInfo)
        {
            StatsInfo = statsInfo;
        }
    }
}
