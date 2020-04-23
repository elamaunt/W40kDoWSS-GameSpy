using System.Windows.Controls;
using System.Windows.Input;
using Framework;

namespace ThunderHawk.Core
{
    public class AuthorizationWindowViewModel : WindowViewModel
    {
        
        public TextFrame Login = new TextEditorFrame();
        public TextFrame Password = new TextEditorFrame();
        public ToggleButtonFrame ShouldLaunchGame { get; set; } = new ToggleButtonFrame() { Text = CoreContext.LangService.GetString("ShouldLaunchGame") };

        public ButtonFrame Close { get; } = new ButtonFrame() {Text = " x "};
        public ButtonFrame Authorize { get; } = new ButtonFrame() { Text = CoreContext.LangService.GetString("Authorize") };
        public ButtonFrame CreateAnotherAccount { get; } = new ButtonFrame() { Text = CoreContext.LangService.GetString("CreateAnotherAccount") };
        public TextFrame AccountsLabel { get; } = new TextFrame();
        
    }
}
