using Framework;

namespace ThunderHawk.Core
{
    public class GameItemViewModel : ItemViewModel
    {
        public TextFrame UploadedName { get; } = new TextFrame();
        public TextFrame Type { get; } = new TextFrame();
        public PlayerFrame Player0 { get; } = new PlayerFrame();
        public PlayerFrame Player1 { get; } = new PlayerFrame();
        public PlayerFrame Player2 { get; } = new PlayerFrame();
        public PlayerFrame Player3 { get; } = new PlayerFrame();
        public PlayerFrame Player4 { get; } = new PlayerFrame();
        public PlayerFrame Player5 { get; } = new PlayerFrame();
        public PlayerFrame Player6 { get; } = new PlayerFrame();
        public PlayerFrame Player7 { get; } = new PlayerFrame();
    }
}
