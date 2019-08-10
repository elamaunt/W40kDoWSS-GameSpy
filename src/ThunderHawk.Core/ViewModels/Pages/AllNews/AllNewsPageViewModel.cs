using Framework;

namespace ThunderHawk.Core
{
    public class AllNewsPageViewModel : EmbeddedPageViewModel
    {
        public TextFrame Title { get; } = new TextFrame() { Text = "All News" };
    }
}
