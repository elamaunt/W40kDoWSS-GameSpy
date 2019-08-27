using Framework;

namespace ThunderHawk.Core
{
    public class MainPageViewModel : EmbeddedPageViewModel
    {
        public ControlFrame LoadingIndicator { get; } = new ControlFrame() { Visible = false };
        public ListFrame<NewsItemViewModel> News { get; } = new ListFrame<NewsItemViewModel>();
        public ButtonFrame LaunchGame { get; } = new ButtonFrame() { Text = "Launch game" };

        public ActionFrame FAQLabel { get; } = new ActionFrame();

        public ActionFrame Tweaks { get; } = new ActionFrame();

        public TextFrame ErrorsType { get; } = new TextFrame();

        public TextFrame ActiveModRevision { get; } = new TextFrame() { Visible = false };

        public ActionFrame FoundErrors { get; } = new ActionFrame();

        public MainPageViewModel()
        {
            TitleButton.Text = CoreContext.LangService.GetString("MainPage").ToUpperInvariant();
        }
    }
}
