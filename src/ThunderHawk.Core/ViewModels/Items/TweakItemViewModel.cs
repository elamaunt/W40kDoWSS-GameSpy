using Framework;
using ThunderHawk.Core.Services;

namespace ThunderHawk.Core
{
    public class TweakItemViewModel : ItemViewModel
    {
        public TextFrame Name { get; } = new TextFrame();
        public TextFrame Description { get; } = new TextFrame();
        public ToggleFrame ShouldApplyTweak { get; } = new ToggleFrame();

        public ITweak RawTweak { get; }
        //public TextFrame ApplyText { get; } = new TextFrame();
        //public TextFrame RestoreText { get; } = new TextFrame();

        public TweakItemViewModel(ITweak tweak, bool shouldApplyTweak/*, string applyText, string restoreText*/)
        {
            RawTweak = tweak;
            Name.Text = tweak.Title;
            Description.Text = tweak.Description;
            ShouldApplyTweak.IsChecked = shouldApplyTweak;
            //ApplyText.Text = applyText;
            //RestoreText.Text = restoreText;
        }
    }
}
