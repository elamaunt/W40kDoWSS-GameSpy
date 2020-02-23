using Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

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

                                itemVM.Navigate.Action = () =>
                                    Frame.GlobalNavigationManager?.OpenPage<NewsViewerPageViewModel>(bundle =>
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
            Frame.LaunchGame.Text = "Launch Thunderhawk\n          (1x1, 2x2)";
            Frame.LaunchGame.Action = LaunchThunderhawk;
            Frame.LaunchSteamGame.Action = LaunchSteam;
        }

        void OpenFAQ()
        {
            Frame.GlobalNavigationManager.OpenPage<FAQPageViewModel>();
        }

        void OpenTweaks()
        {
            Frame.GlobalNavigationManager.OpenPage<TweaksPageViewModel>();
        }

        void LaunchThunderhawk()
        {
            Frame.LaunchGame.Enabled = false;
            Frame.LaunchSteamGame.Enabled = false;
            Frame.LaunchGame.Text = "Thunderhawk launched";
            Frame.LaunchSteamGame.Text = "Thunderhawk launched";

            CoreContext.LaunchService.LaunchThunderHawkGameAndWait("thunderhawk")
                .OnFaultOnUi(ex => Frame.UserInteractions.ShowErrorNotification(ex.Message))
                .OnContinueOnUi(t => { ResetButtons(); });
        }

        void LaunchSteam()
        {
            Frame.LaunchGame.Enabled = false;
            Frame.LaunchSteamGame.Enabled = false;
            Frame.LaunchGame.Text = "Steam launched";
            Frame.LaunchSteamGame.Text = "Steam launched";

            Task.Delay(10000).ContinueWith(t => RunOnUIThread(() => { ResetButtons(); }));

            CoreContext.LaunchService.LaunchThunderHawkGameAndWait("steam");
        }

        void ResetButtons()
        {
            Frame.LaunchGame.Text = "Launch Thunderhawk\n          (1x1, 2x2)";
            Frame.LaunchSteamGame.Text = "Launch Steam\n(3x3, 4x4, ffa)";
            Frame.LaunchGame.Enabled = true;
            Frame.LaunchSteamGame.Enabled = true;
        }
    }
}