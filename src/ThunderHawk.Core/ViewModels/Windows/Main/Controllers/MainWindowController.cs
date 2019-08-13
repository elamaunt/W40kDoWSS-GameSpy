using Framework;

namespace ThunderHawk.Core
{
    class MainWindowController : BindingController<IMainWindow, MainWindowViewModel>
    {
        protected override void OnBind()
        {
            Frame.OpenSettings.Action = OpenSettings;
        }

        void OpenSettings()
        {
            View.OpenWindow(new SettingsWindowViewModel());
        }
    }
}
