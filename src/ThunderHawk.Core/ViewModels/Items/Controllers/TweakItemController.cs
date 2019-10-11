using Framework;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ThunderHawk.Core
{
    class TweakItemController : FrameController<TweakItemViewModel>
    {
        private bool _shouldSwitchTweak = true;
        protected override void OnBind()
        {
            try
            {
                // Check it without subscription
                Frame.IsTweakEnabled.IsChecked = Frame.RawTweak.CheckTweak();
                SubscribeOnPropertyChanged(Frame.IsTweakEnabled, nameof(Frame.IsTweakEnabled.IsChecked), OnCheckedChanged);
            }
            catch (Exception ex)
            {
                //TODO: Какое-нибудь оповещение для юзера что в твиках произошла ошибка. Скорее всего, из-за того что он удалил что-то из GameFiles.
                Logger.Error(ex);

                // Still didn't subscribe!
                Frame.IsTweakEnabled.IsChecked = false;
            }

            SubscribeOnPropertyChanged(Frame.IsTweakEnabled, nameof(Frame.IsTweakEnabled.IsChecked), OnCheckedChanged);
        }

        void OnCheckedChanged()
        {
            if (_shouldSwitchTweak == false)
            {
                _shouldSwitchTweak = true;
                return;
            }

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
                Logger.Error(ex);
                //TODO: Какое-нибудь оповещение для юзера что в твиках произошла ошибка. Скорее всего, из-за отсутствия прав админа или типа-того.

                _shouldSwitchTweak = false;
                Frame.IsTweakEnabled.IsChecked = !Frame.IsTweakEnabled.IsChecked;
            }
        }
    }
}
