using Framework;
using System;
using System.Linq;

namespace ThunderHawk.Core
{
    public class MainPageController : FrameController<MainPageViewModel>
    {
        protected override void OnBind()
        {
            Frame.Title.Text = CoreContext.LangService.GetString("MainPage");

            Frame.LaunchGame.Action = LaunchGame;
            Frame.FAQLabel.Action = OpenFAQ;
            Frame.Tweaks.Action = OpenTweaks;


            var foundRecommendedTweaks = CoreContext.TweaksService.GetState();
            if (foundRecommendedTweaks)
            {
                Frame.FoundErrors.Visible = true;
                Frame.ErrorsType.Text = CoreContext.LangService.GetString("FoundErrors");
            }
            else
            {
                Frame.FoundErrors.Visible = false;
                Frame.ErrorsType.Text = "";
            }

            if (Frame.News.DataSource.IsNullOrEmpty())
            {
                CoreContext.NewsProvider.GetNews()
                    .OnCompletedOnUi(news =>
                    {
                        var newsSource = news.Select(x =>
                        {
                            var itemVM = new NewsItemViewModel(x);

                            itemVM.Navigate.Action = () => Frame.GlobalNavigationManager?.OpenPage<NewsViewerPageViewModel>(bundle =>
                            {
                                bundle.SetString(nameof(NewsViewerPageViewModel.NewsItem), x.AsJson());
                            });

                            return itemVM;
                        })
                        .ToObservableCollection();

                        newsSource[0].Big = true;
                        Frame.News.DataSource = newsSource;
                    })
                    .AttachIndicator(Frame.LoadingIndicator);
            }
        }

        void OpenFAQ()
        {
            Frame.GlobalNavigationManager.OpenPage<FAQPageViewModel>();
        }

        void OpenTweaks()
        {
            Frame.GlobalNavigationManager.OpenPage<TweaksPageViewModel>();
        }

        void LaunchGame()
        {
            CoreContext.LaunchService.LaunchGame();
        }
    }
}
