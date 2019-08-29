using Framework;

namespace ThunderHawk.Core
{
    public abstract class EmbeddedPageViewModel : PageViewModel
    {
        public override ITextFrame Title => TitleButton;
        public ToggleButtonFrame TitleButton { get; } = new ToggleButtonFrame();

        public EmbeddedPageViewModel()
        {
            RequestViewExtention<IUserInteractions>();
        }

        public override string GetPrefix()
        {
            return "element";
        }
    }
}
