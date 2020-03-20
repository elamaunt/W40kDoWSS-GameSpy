using Framework;

namespace ThunderHawk.Core
{
    public class RegistrationWindowViewModel : WindowViewModel
    {
        public ButtonFrame Register { get; } = new ButtonFrame { Text = CoreContext.LangService.GetString("Register") };
        public TextFrame HelpLabel { get; } = new TextFrame();

    }
}
