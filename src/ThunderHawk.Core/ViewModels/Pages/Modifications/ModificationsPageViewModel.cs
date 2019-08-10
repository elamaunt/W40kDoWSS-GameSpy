using Framework;

namespace ThunderHawk.Core
{
    public class ModificationsPageViewModel : EmbeddedPageViewModel
    {
        public TextFrame Title { get; } = new TextFrame() { Text = "Modifications" };
    }
}
