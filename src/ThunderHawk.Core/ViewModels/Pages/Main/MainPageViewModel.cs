using Framework;

namespace ThunderHawk.Core
{
    public class MainPageViewModel : EmbeddedPageViewModel
    {
        public ListFrame<NewsItemViewModel> News { get; } = new ListFrame<NewsItemViewModel>();
        public ActionFrame LaunchGame { get; } = new ActionFrame();

        public ActionFrame FAQLabel { get; } = new ActionFrame();
        public ActionFrame HelpingLabel { get; } = new ActionFrame();
        public ToggleFrame ErrorsFound { get; } = new ToggleFrame();
        public MainPageViewModel()
        {
            TitleButton.Text = "Main";
        }
    }
}
