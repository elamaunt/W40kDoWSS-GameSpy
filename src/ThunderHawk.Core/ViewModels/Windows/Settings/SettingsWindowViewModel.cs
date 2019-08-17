using Framework;
using System;

namespace ThunderHawk.Core
{
    public class SettingsWindowViewModel : WindowViewModel
    {
        public ToggleButtonFrame DisableFog { get; set; } = new ToggleButtonFrame() { Text="Disable fog in game"  };
    }
}
