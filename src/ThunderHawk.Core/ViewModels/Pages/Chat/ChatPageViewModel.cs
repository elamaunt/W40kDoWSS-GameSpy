using Framework;

namespace ThunderHawk.Core
{
    public class ChatPageViewModel : EmbeddedPageViewModel
    {
        public TextFrame Title { get; } = new TextFrame() { Text = "Chat" };
    }
}
