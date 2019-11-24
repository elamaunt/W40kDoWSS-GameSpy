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

            Frame.FAQLabel.Action = OpenFAQ;
            Frame.Tweaks.Action = OpenTweaks;

            try
            {
                Frame.Tweaks.Visible = CoreContext.TweaksService.RecommendedTweaksExists();
            }
            catch (Exception ex)
            {
                //TODO: Какое-нибудь оповещение для юзера что в твиках произошла ошибка. Скорее всего, из-за того что он удалил что-то из GameFiles.
                Logger.Error(ex);
            }

            RecreateToken();

            if (Frame.News.DataSource.IsNullOrEmpty())
            {
                CoreContext.NewsProvider.LoadLastNews(Token)
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

            Frame.ActiveModRevision.Text = $"Thunderhawk <b>" + CoreContext.ThunderHawkModManager.ModVersion + "</b>";
            Frame.LaunchGame.Text = "Launch Thunderhawk";
            Frame.LaunchGame.Action = LaunchGame;
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
            Frame.LaunchGame.Enabled = false;
            Frame.LaunchGame.Text = "Game launched";

            CoreContext.LaunchService.LaunchThunderHawkGameAndWait()
                .OnFaultOnUi(ex => Frame.UserInteractions.ShowErrorNotification(ex.Message))
                .OnContinueOnUi(t =>
                {
                    Frame.LaunchGame.Text = "Launch Thunderhawk";
                    Frame.LaunchGame.Enabled = true;
                });
        }
    }
}
