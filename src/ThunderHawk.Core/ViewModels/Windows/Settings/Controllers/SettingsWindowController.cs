using Framework;

namespace ThunderHawk.Core
{
    class SettingsWindowController : FrameController<SettingsWindowViewModel>
    {
        protected override void OnBind()
        {
            Frame.DisableFog.IsChecked = true;
        }
    }
}
