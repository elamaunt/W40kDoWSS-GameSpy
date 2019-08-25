using Framework;

namespace ThunderHawk.Core
{
    public class SettingsWindowViewModel : WindowViewModel
    {
        public ToggleButtonFrame ThunderHawkModAutoSwitch { get; set; } = new ToggleButtonFrame() { Text = "Autoswitch to ThunderHawk mod on game launch" };
    }
}
