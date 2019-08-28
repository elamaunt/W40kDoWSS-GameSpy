using Framework;
using System;

namespace ThunderHawk.Core
{
    class TweakItemController : FrameController<TweakItemViewModel>
    {
        protected override void OnBind()
        {
            try
            {
                Frame.IsTweakEnabled.IsChecked = Frame.RawTweak.CheckTweak();
                SubscribeOnPropertyChanged(Frame.IsTweakEnabled, nameof(Frame.IsTweakEnabled.IsChecked), OnCheckedChanged);
            }
            catch (Exception ex)
            {
                //TODO: Какое-нибудь оповещение для юзера что в твиках произошла ошибка. Скорее всего, из-за того что он удалил что-то из GameFiles.
                Logger.Error(ex);
                Frame.IsTweakEnabled.IsChecked = false;
            }
        }

        void OnCheckedChanged()
        {
            if (Frame.IsTweakEnabled.IsChecked ?? false)
                Frame.RawTweak.EnableTweak();
            else
                Frame.RawTweak.DisableTweak();
            Frame.OnTweakChanged?.Invoke();
        }
    }
}
