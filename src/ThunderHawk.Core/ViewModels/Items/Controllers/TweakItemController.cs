using Framework;

namespace ThunderHawk.Core
{
    class TweakItemController : FrameController<TweakItemViewModel>
    {
        protected override void OnBind()
        {
            Frame.IsTweakEnabled.IsChecked = Frame.RawTweak.CheckTweak();
            SubscribeOnPropertyChanged(Frame.IsTweakEnabled, nameof(Frame.IsTweakEnabled.IsChecked), OnCheckedChanged);
        }

        void OnCheckedChanged()
        {
            if (Frame.IsTweakEnabled.IsChecked ?? false)
                Frame.RawTweak.EnableTweak();
            else
                Frame.RawTweak.DisableTweak();
        }
    }
}
