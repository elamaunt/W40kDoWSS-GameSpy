using Framework;
using System;
using ThunderHawk.Core.Services;

namespace ThunderHawk.Core
{
    public class TweakItemViewModel : ItemViewModel
    {
        public TextFrame Name { get; } = new TextFrame();
        public TextFrame Description { get; } = new TextFrame();
        public ToggleFrame IsTweakEnabled { get; } = new ToggleFrame();
        //public TextFrame IsRecommended { get; } = new TextFrame();

        public Action OnTweakChanged;

        public ITweak RawTweak { get; }

        public ControlFrame GridMargin { get; } = new ControlFrame();

        public TweakItemViewModel(ITweak tweak)
        {
            RawTweak = tweak;
            Name.Text = tweak.TweakTitle;
            Description.Text = tweak.TweakDescription;
        }
    }
}
