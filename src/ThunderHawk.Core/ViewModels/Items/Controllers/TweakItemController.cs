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
            }
            catch (Exception ex)
            {
                //TODO: Какое-нибудь оповещение для юзера что в твиках произошла ошибка. Скорее всего, из-за того что он удалил что-то из GameFiles.
                Logger.Error(ex);
                Frame.IsTweakEnabled.IsChecked = false;
            }

            SubscribeOnPropertyChanged(Frame.IsTweakEnabled, nameof(Frame.IsTweakEnabled.IsChecked), OnCheckedChanged);
        }

        void OnCheckedChanged()
        {
            try
            {
                if (Frame.IsTweakEnabled.IsChecked ?? false)
                    Frame.RawTweak.EnableTweak();
                else
                    Frame.RawTweak.DisableTweak();
                Frame.OnTweakChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Frame.IsTweakEnabled.IsChecked = !Frame.IsTweakEnabled.IsChecked;
                //TODO: Какое-нибудь оповещение для юзера что в твиках произошла ошибка. Скорее всего, из-за отсутствия прав админа или типа-того.
                Logger.Error(ex);
            }
        }
    }
}
