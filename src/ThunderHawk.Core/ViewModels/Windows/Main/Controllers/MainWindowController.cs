using Framework;

namespace ThunderHawk.Core
{
    class MainWindowController : FrameController<MainWindowViewModel>
    {
        protected override void OnBind()
        {
            Frame.OpenSettings.Action = OpenSettings;
            Frame.UserAccount.Text = CoreContext.SteamApi.NickName;
        }

        void OpenSettings()
        {
            Frame.GlobalNavigationManager.OpenWindow<SettingsWindowViewModel>();
        }
    }
}
