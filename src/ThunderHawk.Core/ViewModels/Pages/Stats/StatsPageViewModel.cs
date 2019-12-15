using Framework;

namespace ThunderHawk.Core
{ 
    public class StatsPageViewModel : EmbeddedPageViewModel
    {
        public ControlFrame LoadingIndicator { get; } = new ControlFrame();
        public TextFrame Rating { get; } = new TextFrame();
        public ListFrame<GameItemViewModel> LastGames { get; } = new ListFrame<GameItemViewModel>();
        public ListFrame<PlayerItemViewModel> Top10Players { get; } = new ListFrame<PlayerItemViewModel>();

        public StatsPageViewModel()
        {            TitleButton.Text = CoreContext.LangService.GetString("StatsPage");
        }
    }
}
