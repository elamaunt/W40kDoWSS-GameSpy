using Framework;

namespace ThunderHawk.Core
{
    public abstract class EmbeddedPageViewModel : PageViewModel
    {
        public override string GetPrefix()
        {
            return "element";
        }
    }
}
