using Framework;

namespace ThunderHawk.Core
{
    public class ChatUserItemViewModel : ItemViewModel
    {
        public TextFrame Name { get; } = new TextFrame();
        public ValueFrame<bool> ActiveProfile { get; } = new ValueFrame<bool>();
        public UserInfo Info { get; }

        public ChatUserItemViewModel(UserInfo userInfo)
        {
            Info = userInfo;
            Name.Text = userInfo.UIName;
        }
    }
}
