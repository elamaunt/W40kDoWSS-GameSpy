using Framework;

namespace ThunderHawk.Core
{
    public class PlayerFrame : ControlFrame
    {
        public TextFrame Name { get; } = new TextFrame();
        public TextFrame Rating { get; } = new TextFrame();
        public TextFrame Race { get; } = new TextFrame();
        public TextFrame Team { get; } = new TextFrame();
        public TextFrame FinalState { get; } = new TextFrame();
    }
}
