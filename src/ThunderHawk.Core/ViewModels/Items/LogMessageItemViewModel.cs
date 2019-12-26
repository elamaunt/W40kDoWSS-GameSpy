using Framework;

namespace ThunderHawk.Core
{
    public class LogMessageItemViewModel : ItemViewModel
    {
        public TextFrame Message { get; } = new TextFrame();

        public LogMessageItemViewModel(string message)
        {
            Message.Text = message;
        }
    }
}
