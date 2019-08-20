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

            UpdateTweaksCount();

            ApplyRecommendedTweaks.Action = ApplyTweaksRecommend;
        }

        void ApplyTweaksRecommend()
        {
            var tweaksToApplyVM = RecommendedTweaks.DataSource.
                Where(x => x.ShouldApplyTweak.IsChecked == true).ToList();
            var tweaksToApply = tweaksToApplyVM.Select(x => x.RawTweak);
            foreach (var tweak in tweaksToApply)
            {
                tweak.ApplyTweak();
            }
            //not working
            foreach (var tweakVM in tweaksToApplyVM)
            {
                RecommendedTweaks.DataSource.Remove(tweakVM);
            }
            UpdateTweaksCount();
        }

        void UpdateTweaksCount()
        {
            RecommendedTweaksCount.Text = $" ({RecommendedTweaks.ItemsCount.ToString()})";
        }
    }
}
