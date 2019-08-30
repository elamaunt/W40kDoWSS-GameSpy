using Framework;

namespace ThunderHawk.Core
{
    class MainWindowController : FrameController<MainWindowViewModel>
    {
        protected override void OnBind()
        {
            Frame.OpenSettings.Action = OpenSettings;
           
            var nickText = CoreContext.SteamApi.NickName;
            if (nickText == "")
                nickText = CoreContext.LangService.GetString("SteamNotLaunched");
            Frame.UserAccount.Text = nickText;
        }

        void OpenSettings()
        {
            Frame.GlobalNavigationManager.OpenWindow<SettingsWindowViewModel>();
        }
    }
}
