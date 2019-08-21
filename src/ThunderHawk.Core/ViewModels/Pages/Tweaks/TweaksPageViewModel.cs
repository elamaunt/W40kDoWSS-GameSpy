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

        public TweaksPageViewModel()
        {
            var tweaks = CoreContext.TweaksService.Tweaks;
            RecommendedTweaks.DataSource = tweaks.Where(i => i.IsRecommendedTweak).
                Select(x => new TweakItemViewModel(x)).ToObservableCollection();

            AllTweaks.DataSource = tweaks.Where(i => !i.IsRecommendedTweak).
                Select(x => new TweakItemViewModel(x)).ToObservableCollection();

            UpdateTweaksCount();

            ApplyRecommendedTweaks.Action = ApplyTweaksRecommend;
        }

        void ApplyTweaksRecommend()
        {
            /*var tweaksToApplyVM = Tweaks.DataSource.
                Where(x => x.ShouldApplyTweak.IsChecked == true).ToList();
            var tweaksToApply = tweaksToApplyVM.Select(x => x.RawTweak);
            foreach (var tweak in tweaksToApply)
            {
                tweak.EnableTweak();
            }
            //not working
            foreach (var tweakVM in tweaksToApplyVM)
            {
                Tweaks.DataSource.Remove(tweakVM);
            }
            UpdateTweaksCount();*/
        }

        void UpdateTweaksCount()
        {
            RecommendedTweaksCount.Text = $" ({RecommendedTweaks.ItemsCount.ToString()})";
        }
    }
}
