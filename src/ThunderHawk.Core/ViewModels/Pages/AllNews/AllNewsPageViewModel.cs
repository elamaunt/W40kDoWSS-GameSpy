using Framework;

namespace ThunderHawk.Core
{
    public class AllNewsPageViewModel : EmbeddedPageViewModel
    {
        public override ITextFrame Title { get; } = new TextFrame() { Text = "All News" };
    }
}
