using Framework;
using System.Linq;

namespace ThunderHawk.Core
{
    public class TweaksPageViewModel: EmbeddedPageViewModel
    {
        public ListFrame<TweakItemViewModel> RecommendedTweaks { get; } = new ListFrame<TweakItemViewModel>();

        public ListFrame<TweakItemViewModel> AllTweaks { get; } = new ListFrame<TweakItemViewModel>();

        public TextFrame RecommendedTweaksCount { get; } = new TextFrame();

        public ActionFrame ApplyRecommendedTweaks { get; } = new ActionFrame();

        public ControlFrame ApplyTweaksSP { get; } = new ControlFrame();

        //public ControlFrame AllTweaksAreOkSP { get; } = new ControlFrame();

        public TweaksPageViewModel()
        {
            TitleButton.Text = CoreContext.LangService.GetString("TweaksPage");
        }

    }
}
