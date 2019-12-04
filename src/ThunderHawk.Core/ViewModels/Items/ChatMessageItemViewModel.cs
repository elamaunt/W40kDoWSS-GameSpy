using Framework;

namespace ThunderHawk.Core
{
    public class ChatMessageItemViewModel : ItemViewModel
    {
        public TextFrame Message { get; } = new TextFrame();

        public ChatMessageItemViewModel(MessageInfo info)
        {
            if (info.IsPrivate)
                Message.Text = $"--------- SERVER: {info.Text}";
            else
                Message.Text = $"[{info.Date.ToString("hh:mm:ss")}] {info.Author.UIName}: {info.Text}";
        }
    } 
}
