using Framework;

namespace ThunderHawk.Core
{
    public class ChatUserItemViewModel : ItemViewModel
    {
        public TextFrame Name { get; } = new TextFrame();
        public UserInfo Info { get; }

        public ChatUserItemViewModel(UserInfo userInfo)
        {
            Info = userInfo;
            Name.Text = userInfo.UIName;
        }
    }
}
