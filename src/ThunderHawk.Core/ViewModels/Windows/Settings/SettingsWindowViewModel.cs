using Framework;

namespace ThunderHawk.Core
{
    public class SettingsWindowViewModel : WindowViewModel
    {
        public ToggleButtonFrame ThunderHawkModAutoSwitch { get; set; } = new ToggleButtonFrame() { Text = "Autoswitch to ThunderHawk mod on game launch" };
        public ToggleButtonFrame DesktopNotificationsEnabled { get; set; } = new ToggleButtonFrame() { Text = CoreContext.LangService.GetString("ChangeGamePath") };
        public ToggleButtonFrame LaunchThunderHawkAtStartup { get; set; } = new ToggleButtonFrame() { Text = "Launch ThunderHawk at startup" };
        public ToggleButtonFrame LimitRatingLobby { get; set; } = new ToggleButtonFrame() { Text = CoreContext.LangService.GetString("EnableLobbyFiltering") };
        public ButtonFrame ChangeGamePath { get; } = new ButtonFrame() { Text = CoreContext.LangService.GetString("ChangeGamePath") };
        public TextFrame GamePath { get; } = new TextFrame() { Text = CoreContext.LaunchService.GamePath ?? "Game not found" };

    }
}
