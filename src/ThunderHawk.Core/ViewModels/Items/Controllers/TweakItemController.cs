using System;
using System.ComponentModel;
using Framework;

namespace ThunderHawk.Core
{
    class TweakItemController : FrameController<TweakItemViewModel>
    {
        protected override void OnBind()
        {
            Frame.IsTweakEnabled.IsChecked = Frame.RawTweak.CheckTweak();
            Frame.IsTweakEnabled.PropertyChanged += IsTweakEnabled_PropertyChanged;
        }

        private void IsTweakEnabled_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Frame.IsTweakEnabled.IsChecked == true)
            {
                Frame.RawTweak.EnableTweak();
            }
            else if (Frame.IsTweakEnabled.IsChecked == false)
            {
                Frame.RawTweak.DisableTweak();
            }
        }
    }
}
