using Framework;

namespace ThunderHawk.Core
{
    public class ChatUserItemViewModel : ItemViewModel
    {
        public TextFrame Name { get; } = new TextFrame();

        public ChatUserItemViewModel(UserInfo userInfo)
        {
            Name.Text = userInfo.UIName;
        }
    }
}
