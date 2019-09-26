using Framework;

namespace ThunderHawk.Core
{
    public class ChatMessageItemViewModel : ItemViewModel
    {
        public TextFrame Message { get; } = new TextFrame();

        public ChatMessageItemViewModel(MessageInfo info)
        {
            Message.Text = $"{info.Author.UIName}: {info.Text}";
        }
    } 
}
