using Framework;
using System.Linq;

namespace ThunderHawk.Core
{
    public class TweaksPageViewModel: EmbeddedPageViewModel
    {
        public ListFrame<TweakItemViewModel> RecommendedTweaks { get; } = new ListFrame<TweakItemViewModel>();

        public TextFrame RecommendedTweaksCount { get; } = new TextFrame();

        public ActionFrame ApplyRecommendedTweaks { get; } = new ActionFrame();

        public TweaksPageViewModel()
        {
            var wrongTweaks = CoreContext.TweaksService.GetWrongTweaks();
            RecommendedTweaks.DataSource = 
                wrongTweaks.Select(x => new TweakItemViewModel(x, true)).ToObservableCollection();
            RecommendedTweaksCount.Text = $" ({wrongTweaks.Length.ToString()})";

            ApplyRecommendedTweaks.Action = ApplyTweaksRecommend;
        }

        void ApplyTweaksRecommend()
        {
            var tweaksToApply = RecommendedTweaks.DataSource.
                Where(x => x.ShouldApplyTweak.IsChecked == true).Select(t => t.RawTweak);
            foreach (var tweak in tweaksToApply)
            {
                tweak.ApplyTweak();
            }
        }
    }
}
