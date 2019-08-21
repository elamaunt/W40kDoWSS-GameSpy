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
        //public TextFrame ApplyText { get; } = new TextFrame();
        //public TextFrame RestoreText { get; } = new TextFrame();

        public TweakItemViewModel(ITweak tweak/*, string applyText, string restoreText*/)
        {
            RawTweak = tweak;
            Name.Text = tweak.TweakTitle;
            Description.Text = tweak.TweakDescription;
            //ApplyText.Text = applyText;
            //RestoreText.Text = restoreText;
            IsRecommended.Visible = false;
            IsTweakEnabled.IsChecked = RawTweak.CheckTweak();
            IsTweakEnabled.PropertyChanged += IsTweakEnabled_PropertyChanged;
        }

        private void IsTweakEnabled_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (IsTweakEnabled.IsChecked == true)
            {
                RawTweak.EnableTweak();
            }
            else if (IsTweakEnabled.IsChecked == false)
            {
                RawTweak.DisableTweak();
            }
        }
    }
}
