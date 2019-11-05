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

            if (CoreContext.ThunderHawkModManager.CheckIsModExists())
            {
                UpdateMod();
            }
            else
            {
                Frame.LaunchGame.Text = "Setup mod";
                Frame.LaunchGame.Action = SetupGameMod;
            }
        }

        void UpdateMod()
        {
            if (!CoreContext.LaunchService.TryGetOrChoosePath(out string path))
                return;

            Frame.LaunchGame.Enabled = false;
            CoreContext.ThunderHawkModManager.UpdateMod(path, Token, ReportProgress)
                .OnContinueOnUi(task =>
                {
                    Frame.LaunchGame.Enabled = true;
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        SetGameLaunchState();
                        UpdateActiveModState();
                    }
                });
        }

        private void SetGameLaunchState()
        {
            Frame.LaunchGame.Text = "Launch Thunderhawk";
            Frame.LaunchGame.Action = LaunchGame;
        }

        void SetupGameMod()
        {
            Frame.LaunchGame.Enabled = false;
            var path = CoreContext.LaunchService.GamePath;

            AppSettings.ThunderHawkModAutoSwitch = true;
            CoreContext.ThunderHawkModManager.DownloadMod(path, Token, ReportProgress)
                .OnContinueOnUi(task =>
                {
                    Frame.LaunchGame.Enabled = true;
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        SetGameLaunchState();
                        UpdateActiveModState();
                    }
                });
        }

        void ReportProgress(float percent)
        {
            RunOnUIThread(() =>
            {
                Frame.LaunchGame.Text = "Loading.. " + ((int)(percent * 100)) + "%";
            });
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

        void UpdateActiveModState()
        {
            Frame.ActiveModRevision.Visible = true;
            Frame.ActiveModRevision.Text = $"{CoreContext.LangService.GetString("Active revision")} <b>" + CoreContext.ThunderHawkModManager.ActiveModRevision?.Replace("\n", "")+"</b>";
        }
    }
}
