using Framework;
using System.IO;
using System.Reflection;

namespace ThunderHawk.Core
{
    class MainWindowController : FrameController<MainWindowViewModel>
    {
        protected override void OnBind()
        {
            Frame.OpenSettings.Action = OpenSettings;

            if (CoreContext.LaunchService.GamePath == Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                CoreContext.SteamApi.Initialize();

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
