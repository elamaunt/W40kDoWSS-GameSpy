using Framework;

namespace ThunderHawk.Core
{
    class SettingsWindowController : FrameController<SettingsWindowViewModel>
    {
        protected override void OnBind()
        {
            Frame.ThunderHawkModAutoSwitch.IsChecked = AppSettings.ThunderHawkModAutoSwitch;

            SubscribeOnPropertyChanged(Frame.ThunderHawkModAutoSwitch, nameof(IToggleFrame.IsChecked),
                () => AppSettings.ThunderHawkModAutoSwitch = Frame.ThunderHawkModAutoSwitch.IsChecked ?? false);
        }
    }
}
