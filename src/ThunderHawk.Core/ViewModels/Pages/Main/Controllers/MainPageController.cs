﻿using Framework;
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

            Frame.Tweaks.Visible = false;

            try
            {
                var foundRecommendedTweaks = CoreContext.TweaksService.GetState();
                if (foundRecommendedTweaks)
                {
                    Frame.Tweaks.Visible = true;
                }
            }
            catch (Exception ex)
            {
                //TODO: Какое-нибудь оповещение для юзера что в твиках произошла ошибка. Скорее всего, из-за того что он удалил что-то из LauncherFiles.
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
            CoreContext.ThunderHawkModManager.UpdateMod(path, Token, ReportProgress)
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
            CoreContext.ThunderHawkModManager.DownloadMod(path, Token, ReportProgress)
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
            Frame.LaunchGame.Enabled = false;
            Frame.LaunchGame.Text = "Game launched";
            CoreContext.LaunchService.LaunchGameAndWait()
                .OnContinueOnUi(t =>
                {
                    Frame.LaunchGame.Text = "Launch game";
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
