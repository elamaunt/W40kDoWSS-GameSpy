using Framework;
using System.Collections.ObjectModel;

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

            if (!userInfo.IsUser)
            {
                ContextMenu = new MenuFrame()
                {
                    MenuItems = new ObservableCollection<IMenuItemFrame>()
                    {
                        new MenuButtonFrame(){ Text = "Test connection with this player", Action = OnTestConnectionClicked },
                        new MenuButtonFrame(){ Text = "Reset port binding with this player (dont use if you in game with this player)", Action = OnResetPortClicked }
                    }
                };
            }
        }

        void OnTestConnectionClicked()
        {
            CoreContext.SteamApi.TestConnectionWithPlayer(Info.SteamId);
        }

        void OnResetPortClicked()
        {
            CoreContext.SteamApi.ResetPortBindingWithPlayer(Info.SteamId);
        }
    }
}
