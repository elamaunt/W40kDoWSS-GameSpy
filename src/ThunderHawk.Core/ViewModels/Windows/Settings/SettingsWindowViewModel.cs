using Framework;
using System;

namespace ThunderHawk.Core
{
    public class SettingsWindowViewModel : WindowViewModel
    {
        public ToggleFrame DisableFog { get; set; } = new ToggleFrame();

        public SettingsWindowViewModel()
        {
        }
    }
}
