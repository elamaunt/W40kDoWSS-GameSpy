using Framework;

namespace ThunderHawk.Core
{
    public class ModificationsPageViewModel : EmbeddedPageViewModel
    {
        public override ITextFrame Title { get; } = new TextFrame() { Text = "Modifications" };
    }
}
