using Framework;

namespace ThunderHawk.Core
{
    class SettingsWindowController : BindingController<ISettingsWindow, SettingsWindowViewModel>
    {
        protected override void OnBind()
        {
            Frame.DisableFog.IsChecked = true;
        }
    }
}
