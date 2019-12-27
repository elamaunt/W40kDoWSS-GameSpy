using Framework;

namespace ThunderHawk.Core
{
    public class SettingsWindowViewModel : WindowViewModel
    {
        public ToggleButtonFrame ThunderHawkModAutoSwitch { get; set; } = new ToggleButtonFrame() { Text = "Autoswitch to ThunderHawk mod on game launch" };
        public ToggleButtonFrame DesktopNotificationsEnabled { get; set; } = new ToggleButtonFrame() { Text = "Desktop notifications enabled" };
        public ToggleButtonFrame LaunchThunderHawkAtStartup { get; set; } = new ToggleButtonFrame() { Text = "Launch ThunderHawk at startup" };

        public TextFrame LimitRatingLobbyInfo { get; } = new TextFrame() { Text = "If enabled you will not see Ranked games with more than 180 rating deviation and other players also will not see you lobby with same deviation." };
        public ToggleButtonFrame LimitRatingLobby { get; set; } = new ToggleButtonFrame() { Text = "Enable rating lobby filtering" };
        
        public ButtonFrame ChangeGamePath { get; } = new ButtonFrame() { Text = CoreContext.LangService.GetString("ChangeGamePath") };
        public TextFrame GamePath { get; } = new TextFrame() { Text = CoreContext.LaunchService.GamePath ?? "Game not found" };

    }
}
