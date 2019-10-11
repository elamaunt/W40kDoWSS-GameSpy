using Framework;
using System;
using System.Linq;

namespace ThunderHawk.Core
{
    public class TweaksPageController : FrameController<TweaksPageViewModel>
    {
        protected override void OnBind()
        {
            var tweaks = CoreContext.TweaksService.Tweaks;
            Frame.RecommendedTweaks.DataSource = tweaks.Where(i => i.IsRecommendedTweak).
                Select(x => new TweakItemViewModel(x)).ToObservableCollection();

            Frame.AllTweaks.DataSource = tweaks.Where(i => !i.IsRecommendedTweak).
                Select(x => new TweakItemViewModel(x)).ToObservableCollection();

            Frame.RecommendedTweaks.DataSource[Frame.RecommendedTweaks.DataSource.Count - 1].GridMargin.Visible = false;

            UpdateTweaksCount();

            Frame.ApplyRecommendedTweaks.Action = ApplyTweaksRecommend;

            foreach (var recommendedTweak in Frame.RecommendedTweaks.DataSource)
            {
                recommendedTweak.OnTweakChanged = OnRecommendedTweaksChange;
            }
        }

        private void OnRecommendedTweaksChange()
        {
            UpdateTweaksCount();
        }

        void ApplyTweaksRecommend()
        {
            var tweaksToApply = Frame.RecommendedTweaks.DataSource.Where(t => !t.RawTweak.CheckTweak());
            foreach (var tweak in tweaksToApply)
            {
                tweak.IsTweakEnabled.IsChecked = true;
            }
        }

        void UpdateTweaksCount()
        {
            try
            {
                var tweaksCount = Frame.RecommendedTweaks.DataSource.Select(x => x.RawTweak).Count(t => !t.CheckTweak());
                Frame.RecommendedTweaksCount.Text = $" ({tweaksCount.ToString()})";

                Frame.ApplyTweaksSP.Visible = tweaksCount != 0;
            }
            catch (Exception ex)
            {
                //TODO: Какое-нибудь оповещение для юзера что в твиках произошла ошибка. Скорее всего, из-за того что он удалил что-то из GameFiles.
                Logger.Error(ex);
            }
        }
    }
}
