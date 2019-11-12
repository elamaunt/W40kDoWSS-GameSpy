using Framework;
using System.Linq;

namespace ThunderHawk.Core
{
    public class StatsPageController : FrameController<StatsPageViewModel>
    {
        protected override void OnBind()
        {
            CoreContext.MasterServer.PlayerTopLoaded += OnPlayersTopLoaded;
            CoreContext.MasterServer.RequestPlayersTop(0, 10);
        }

        void OnPlayersTopLoaded(StatsInfo[] players, long offset)
        {
            var source = players.Select(x => new PlayerItemViewModel(x)).ToObservableCollection();

            RunOnUIThread(() =>
            {
                Frame.Top10Players.DataSource = source;
            });
        }
    }
}
