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

            var path = CoreContext.LaunchService.GamePath;

            if (CoreContext.ThunderHawkModManager.CheckIsModExists(path))
            {
                UpdateMod();
            }
            else
            {
                Frame.LaunchGame.Text = "Setup";
                Frame.LaunchGame.Action = SetupGameMod;
            }
        }

        void UpdateMod()
        {
            Frame.LaunchGame.Enabled = false;
            var path = CoreContext.LaunchService.GamePath;
            CoreContext.ThunderHawkModManager.UpdateMod(path, RecreateToken(), ReportProgress)
                .OnContinueOnUi(task =>
                {
                    Frame.LaunchGame.Enabled = true;
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        Frame.LaunchGame.Text = "Launch game";
                        Frame.LaunchGame.Action = LaunchGame;
                        UpdateActiveModState();
                    }
                });
        }

        void SetupGameMod()
        {
            Frame.LaunchGame.Enabled = false;
            var path = CoreContext.LaunchService.GamePath;
            CoreContext.ThunderHawkModManager.DownloadMod(path, RecreateToken(), ReportProgress)
                .OnContinueOnUi(task =>
                {
                    Frame.LaunchGame.Enabled = true;
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        Frame.LaunchGame.Text = "Launch game";
                        Frame.LaunchGame.Action = LaunchGame;
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
            if (AppSettings.ThunderHawkModAutoSwitch)
                CoreContext.LaunchService.SwitchGameToMod(CoreContext.ThunderHawkModManager.ModName);

            Frame.LaunchGame.Enabled = false;
            Frame.LaunchGame.Text = "Game launched";
            CoreContext.LaunchService.LaunchGameAndWait()
                .OnContinueOnUi(t =>
                {
                    Frame.LaunchGame.Enabled = true;
                });
        }

        void UpdateActiveModState()
        {
            Frame.ActiveModRevision.Visible = true;
            Frame.ActiveModRevision.Text = "Активная ревизия: <b>" + CoreContext.ThunderHawkModManager.ActiveModRevision?.Replace("\n", "")+"</b>";
        }
    }
}
