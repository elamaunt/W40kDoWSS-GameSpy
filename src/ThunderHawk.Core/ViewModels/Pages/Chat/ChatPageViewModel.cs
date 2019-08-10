using Framework;

namespace ThunderHawk.Core
{
    public class ChatPageViewModel : EmbeddedPageViewModel
    {
        public override ITextFrame Title { get; } = new TextFrame() { Text = "Chat" };
    }
}
