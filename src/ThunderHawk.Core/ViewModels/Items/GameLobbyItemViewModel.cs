using Framework;

namespace ThunderHawk.Core
{
    public class GameLobbyItemViewModel : ItemViewModel
    {
        public ControlFrame Player0 { get; } = new ControlFrame();
        public ControlFrame Player1 { get; } = new ControlFrame();
        public ControlFrame Player2 { get; } = new ControlFrame();
        public ControlFrame Player3 { get; } = new ControlFrame();
        public ControlFrame Player4 { get; } = new ControlFrame();
        public ControlFrame Player5 { get; } = new ControlFrame();
        public ControlFrame Player6 { get; } = new ControlFrame();
        public ControlFrame Player7 { get; } = new ControlFrame();

        public ControlFrame ActivePlayer0 { get; } = new ControlFrame();
        public ControlFrame ActivePlayer1 { get; } = new ControlFrame();
        public ControlFrame ActivePlayer2 { get; } = new ControlFrame();
        public ControlFrame ActivePlayer3 { get; } = new ControlFrame();
        public ControlFrame ActivePlayer4 { get; } = new ControlFrame();
        public ControlFrame ActivePlayer5 { get; } = new ControlFrame();
        public ControlFrame ActivePlayer6 { get; } = new ControlFrame();
        public ControlFrame ActivePlayer7 { get; } = new ControlFrame();

        public GameHostInfo HostInfo { get; }

        public GameLobbyItemViewModel(GameHostInfo hostInfo)
        {
            HostInfo = hostInfo;

            Player0.Visible = hostInfo.MaxPlayers > 0;
            Player1.Visible = hostInfo.MaxPlayers > 1;
            Player2.Visible = hostInfo.MaxPlayers > 2;
            Player3.Visible = hostInfo.MaxPlayers > 3;
            Player4.Visible = hostInfo.MaxPlayers > 4;
            Player5.Visible = hostInfo.MaxPlayers > 5;
            Player6.Visible = hostInfo.MaxPlayers > 6;
            Player7.Visible = hostInfo.MaxPlayers > 7;

            ActivePlayer0.Visible = hostInfo.Players > 0;
            ActivePlayer1.Visible = hostInfo.Players > 1;
            ActivePlayer2.Visible = hostInfo.Players > 2;
            ActivePlayer3.Visible = hostInfo.Players > 3;
            ActivePlayer4.Visible = hostInfo.Players > 4;
            ActivePlayer5.Visible = hostInfo.Players > 5;
            ActivePlayer6.Visible = hostInfo.Players > 6;
            ActivePlayer7.Visible = hostInfo.Players > 7;
        }
    }
}
