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

            RecommendedTweaks.DataSource.CollectionChanged += DataSource_CollectionChanged;
        }

        private void DataSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateTweaksCount();
        }

        void ApplyTweaksRecommend()
        {
            var tweaksToApply = RecommendedTweaks.DataSource.Where(t => !t.RawTweak.CheckTweak());
            foreach (var tweak in tweaksToApply)
            {
                tweak.IsTweakEnabled.IsChecked = true;
            }
        }

        void UpdateTweaksCount()
        {
            var tweaksCount = CoreContext.TweaksService.Tweaks.Where(t => t.IsRecommendedTweak).Where(t => !t.CheckTweak()).Count();
            RecommendedTweaksCount.Text = $" ({tweaksCount.ToString()})";
        }
    }
}
