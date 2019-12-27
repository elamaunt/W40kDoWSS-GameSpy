using Framework;

namespace ThunderHawk.Core
{
    class SettingsWindowController : FrameController<SettingsWindowViewModel>
    {
        protected override void OnBind()
        {
            Frame.DesktopNotificationsEnabled.IsChecked = AppSettings.DesktopNotificationsEnabled;
            Frame.ThunderHawkModAutoSwitch.IsChecked = AppSettings.ThunderHawkModAutoSwitch;
            Frame.LaunchThunderHawkAtStartup.IsChecked = AppSettings.LaunchThunderHawkAtStartup;
            Frame.LimitRatingLobby.IsChecked = AppSettings.LimitRatingLobby;
            
            SubscribeOnPropertyChanged(Frame.ThunderHawkModAutoSwitch, nameof(IToggleFrame.IsChecked),
                () => AppSettings.ThunderHawkModAutoSwitch = Frame.ThunderHawkModAutoSwitch.IsChecked ?? false);

            SubscribeOnPropertyChanged(Frame.DesktopNotificationsEnabled, nameof(IToggleFrame.IsChecked),
                () => AppSettings.DesktopNotificationsEnabled = Frame.DesktopNotificationsEnabled.IsChecked ?? false);
            
            SubscribeOnPropertyChanged(Frame.LaunchThunderHawkAtStartup, nameof(IToggleFrame.IsChecked),
                () => AppSettings.LaunchThunderHawkAtStartup = Frame.LaunchThunderHawkAtStartup.IsChecked ?? false);

            SubscribeOnPropertyChanged(Frame.LimitRatingLobby, nameof(IToggleFrame.IsChecked),
                () => AppSettings.LimitRatingLobby = Frame.LimitRatingLobby.IsChecked ?? false);

            Frame.ChangeGamePath.Action = ChangePath;
        }

        void ChangePath()
        {
            CoreContext.LaunchService.ChangeGamePath();
            Frame.GamePath.Text = CoreContext.LaunchService.GamePath ?? "Game not found";
        }
    }
}
