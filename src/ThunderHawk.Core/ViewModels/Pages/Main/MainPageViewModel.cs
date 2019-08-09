using Framework;

namespace ThunderHawk.Core
{
    public class MainPageViewModel : PageViewModel
    {
        public TextFrame Title { get; } = new TextFrame();
        public ListFrame<NewsItemViewModel> News { get; } = new ListFrame<NewsItemViewModel>();
        public ActionFrame LaunchGame { get; } = new ActionFrame();

        public override string GetPrefix()
        {
            return "element";
        }
    }
}
