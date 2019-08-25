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
                CoreContext.ThunderHawkModManager.CheckIsLastVersion(path)
                    .OnContinueOnUi(task =>
                    {
                        if (task.Status == TaskStatus.RanToCompletion)
                        {
                            if (task.Result)
                            {
                                Frame.LaunchGame.Text = "Launch game";
                                Frame.LaunchGame.Action = LaunchGame;
                            }
                            else
                            {
                                Frame.LaunchGame.Text = "Update";
                                Frame.LaunchGame.Action = UpdateMod;
                            }
                        }
                        else
                        {
                            Frame.LaunchGame.Text = "Setup";
                            Frame.LaunchGame.Action = SetupGameMod;
                            // TODO: Validate game state
                        }
                    });
            }
            else
            {
                Frame.LaunchGame.Text = "Setup";
                Frame.LaunchGame.Action = SetupGameMod;
            }
        }

        void UpdateMod()
        {
            var path = CoreContext.LaunchService.GamePath;
            CoreContext.ThunderHawkModManager.UpdateMod(path, RecreateToken())
                .OnContinueOnUi(task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        Frame.LaunchGame.Text = "Launch game";
                        Frame.LaunchGame.Action = LaunchGame;
                    }
                });
        }

        void SetupGameMod()
        {
            var path = CoreContext.LaunchService.GamePath;
            CoreContext.ThunderHawkModManager.DownloadMod(path, RecreateToken())
                .OnContinueOnUi(task =>
                {
                    if(task.Status == TaskStatus.RanToCompletion)
                    {
                        Frame.LaunchGame.Text = "Launch game";
                        Frame.LaunchGame.Action = LaunchGame;
                    }
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
            //if (AppSettings.ThunderHawkModAutoSwitch)
            //    CoreContext.LaunchService.SwitchGameToMod(CoreContext.ThunderHawkModManager.ModName);

            CoreContext.LaunchService.LaunchGame();
        }
    }
}
