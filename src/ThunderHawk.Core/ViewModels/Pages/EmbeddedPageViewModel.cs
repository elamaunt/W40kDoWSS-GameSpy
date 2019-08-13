using Framework;

namespace ThunderHawk.Core
{
    public abstract class EmbeddedPageViewModel : PageViewModel
    {
        public override ITextFrame Title => TitleButton;
        public ButtonFrame TitleButton { get; } = new ButtonFrame();

        public override string GetPrefix()
        {
            return "element";
        }
    }
}
