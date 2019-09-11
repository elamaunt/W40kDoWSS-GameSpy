using Framework;

namespace ThunderHawk.Core
{
    public class MainPageViewModel : EmbeddedPageViewModel
    {
        public ControlFrame LoadingIndicator { get; } = new ControlFrame() { Visible = false };
        public ListFrame<NewsItemViewModel> News { get; } = new ListFrame<NewsItemViewModel>();
        public ButtonFrame LaunchGame { get; } = new ButtonFrame() { Text = "Launch Thunderhawk" };
        public ButtonFrame LaunchOriginalGame { get; } = new ButtonFrame() { Text = "Launch original" };
        
        public ActionFrame FAQLabel { get; } = new ActionFrame();

        public ActionFrame Tweaks { get; } = new ActionFrame();

        public TextFrame ActiveModRevision { get; } = new TextFrame() { Visible = false };

        public MainPageViewModel()
        {
            TitleButton.Text = CoreContext.LangService.GetString("MainPage").ToUpperInvariant();
        }
    }
}
