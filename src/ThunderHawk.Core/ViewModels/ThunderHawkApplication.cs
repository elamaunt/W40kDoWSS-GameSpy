using Framework;

namespace ThunderHawk.Core
{
    public class ThunderHawkApplication : ApplicationViewModel
    {
        public ListFrame<PageViewModel> Pages { get; } = new ListFrame<PageViewModel>();
    }
}
