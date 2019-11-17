using Framework;
using SharedServices;

namespace ThunderHawk.Core
{
    public class PlayerFrame : ControlFrame
    {
        public TextFrame Name { get; } = new TextFrame();
        public ValueFrame<long> Rating { get; } = new ValueFrame<long>();
        public ValueFrame<Race> Race { get; } = new ValueFrame<Race>();
        public ValueFrame<long> Team { get; } = new ValueFrame<long>();
        public TextFrame FinalState { get; } = new TextFrame();
    }
}
