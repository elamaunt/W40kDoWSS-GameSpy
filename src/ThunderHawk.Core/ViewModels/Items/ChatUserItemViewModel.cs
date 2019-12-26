using Framework;

namespace ThunderHawk.Core
{
    public class ChatUserItemViewModel : ItemViewModel
    {
        public TextFrame Name { get; } = new TextFrame();
        public ValueFrame<GameState> GameState { get; } = new ValueFrame<GameState>();
        public ValueFrame<UserState> State { get; } = new ValueFrame<UserState>();
        public ValueFrame<bool> ActiveProfile { get; } = new ValueFrame<bool>();
        public UserInfo Info { get; }

        public ChatUserItemViewModel(UserInfo userInfo)
        {
            GameState.Value = (userInfo.BFlags?.Contains("g") ?? false) ? Core.GameState.Playing : Core.GameState.Idle;
            State.Value = userInfo.State;
            Info = userInfo;
            Name.Text = userInfo.UIName;
        }
    }
}
