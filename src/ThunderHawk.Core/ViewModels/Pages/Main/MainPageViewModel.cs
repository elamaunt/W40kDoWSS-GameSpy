using Framework;

namespace ThunderHawk.Core
{
    public class MainPageViewModel : EmbeddedPageViewModel
    {
        public override ITextFrame Title { get; } = new TextFrame() { Text = "Main" };
        public ListFrame<NewsItemViewModel> News { get; } = new ListFrame<NewsItemViewModel>();
        public ActionFrame LaunchGame { get; } = new ActionFrame();
    }
}
