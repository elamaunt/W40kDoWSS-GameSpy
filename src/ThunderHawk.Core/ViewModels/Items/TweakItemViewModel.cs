using Framework;
using ThunderHawk.Core.Services;

namespace ThunderHawk.Core
{
    public class TweakItemViewModel : ItemViewModel
    {
        public TextFrame Name { get; } = new TextFrame();
        public TextFrame Description { get; } = new TextFrame();
        public ToggleFrame IsTweakEnabled { get; } = new ToggleFrame();
        public TextFrame IsRecommended { get; } = new TextFrame();
        public ITweak RawTweak { get; }

        public TweakItemViewModel(ITweak tweak)
        {
            RawTweak = tweak;
            Name.Text = tweak.TweakTitle;
            Description.Text = tweak.TweakDescription;
        }
    }
}
