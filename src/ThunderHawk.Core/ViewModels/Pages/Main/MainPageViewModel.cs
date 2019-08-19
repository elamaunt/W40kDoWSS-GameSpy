using Framework;

namespace ThunderHawk.Core
{
    public class MainPageViewModel : EmbeddedPageViewModel
    {
        public ListFrame<NewsItemViewModel> News { get; } = new ListFrame<NewsItemViewModel>();
        public ActionFrame LaunchGame { get; } = new ActionFrame();

        public ActionFrame FAQLabel { get; } = new ActionFrame();

        public ActionFrame Tweaks { get; } = new ActionFrame();

        public TextFrame ErrorsType { get; } = new TextFrame();

        public ActionFrame FoundErrors { get; } = new ActionFrame();

        public MainPageViewModel()
        {
            TitleButton.Text = "Main";
        }
    }
}
