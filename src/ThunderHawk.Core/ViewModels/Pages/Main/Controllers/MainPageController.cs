using Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharedServices;
using ThunderHawk.Core.Frames;

namespace ThunderHawk.Core
{
    public class MainPageController : FrameController<MainPageViewModel>
    {
        Timer _timer;
        PlayerFrameInGame[] playerViewInGames = new PlayerFrameInGame[8];

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
            Frame.LaunchGame.Action = LaunchThunderhawk;
            Frame.LaunchSteamGame.Action = LaunchSteam;

            // InGamePage
            Frame.InGameLabel.Text = "Refresh";
            playerViewInGames[0] = Frame.Player0;
            playerViewInGames[1] = Frame.Player1;
            playerViewInGames[2] = Frame.Player2;
            playerViewInGames[3] = Frame.Player3;
            playerViewInGames[4] = Frame.Player4;
            playerViewInGames[5] = Frame.Player5;
            playerViewInGames[6] = Frame.Player6;
            playerViewInGames[7] = Frame.Player7;

            foreach (var playerFrame in playerViewInGames)
            {
                playerFrame.LoadBackground.BackgroundOpacity = 0;
            }

            Frame.Change.Action = ShowGame;

            _timer = new Timer(InGameOnTime, null, 1000, 1000);
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
            Frame.NewsVisible = false;
            Frame.InGameVisible = true;
            

            CoreContext.LaunchService.LaunchGameAndWait("thunderhawk")
                .OnFaultOnUi(ex => Frame.UserInteractions.ShowErrorNotification(ex.Message))
                .OnContinueOnUi(t =>
                {
                    ResetButtons();
                    Frame.NewsVisible = true;
                    Frame.InGameVisible = false;
                });
        }

        void LaunchSteam()
        {
            Frame.LaunchGame.Enabled = false;
            Frame.LaunchSteamGame.Enabled = false;
            Frame.LaunchGame.Text = "Steam launched";
            Frame.LaunchSteamGame.Text = "Steam launched";

            //TODO: пофиксить этот костыль нормальным определением запущенного стим СС
            Task.Delay(10000).ContinueWith(t => RunOnUIThread(() => { ResetButtons(); }));

            CoreContext.LaunchService.LaunchGameAndWait("steam")
                .OnFaultOnUi(ex => Frame.UserInteractions.ShowErrorNotification(ex.Message));
        }

        void ResetButtons()
        {
            Frame.LaunchGame.Text = "Launch Thunderhawk\n          (1x1, 2x2)";
            Frame.LaunchSteamGame.Text = "Launch Steam\n(3x3, 4x4, ffa)";
            Frame.LaunchGame.Enabled = true;
            Frame.LaunchSteamGame.Enabled = true;
        }

        /*
        ╔══╗╔═╗─╔╗     ╔═══╗╔═══╗╔═╗╔═╗╔═══╗     ╔════╗╔═══╗╔══╗─
        ╚╣─╝║║╚╗║║     ║╔═╗║║╔═╗║║║╚╝║║║╔══╝     ║╔╗╔╗║║╔═╗║║╔╗║─
        ─║║─║╔╗╚╝║     ║║─╚╝║║─║║║╔╗╔╗║║╚══╗     ╚╝║║╚╝║║─║║║╚╝╚╗
        ─║║─║║╚╗║║     ║║╔═╗║╚═╝║║║║║║║║╔══╝     ──║║──║╚═╝║║╔═╗║
        ╔╣─╗║║─║║║     ║╚╩═║║╔═╗║║║║║║║║╚══╗     ──║║──║╔═╗║║╚═╝║
        ╚══╝╚╝─╚═╝     ╚═══╝╚╝─╚╝╚╝╚╝╚╝╚═══╝     ──╚╝──╚╝─╚╝╚═══╝    
         */

        void InGameOnTime(object state)
        {
            RunOnUIThread(() =>
            {
                if (CoreContext.InGameService.errorOccured)
                {
                    Frame.InfoLabel.Text = "Fatal error occured. Pls send launcher log to developers";
                    SetTabsToDefault();
                }
                else
                {
                    if (CoreContext.InGameService.isGameNow && CoreContext.LaunchService.GameProcess != null)
                    {
                        ShowGame();
                        Frame.InfoLabel.Text = "";
                    }
                    else
                    {
                        if (CoreContext.LaunchService.GameProcess == null)
                        {
                            Frame.InfoLabel.Text = "Can't subscribe to soulstorm process";
                        }
                        else Frame.InfoLabel.Text = "Successful subscribe to Soulstorm - waiting for the game start";
                        
                        if (CoreContext.LaunchService.IsGamePreparingToStart) Frame.InfoLabel.Text = "Preparing thunderhawk mod, pls wait...";
                        

                        SetTabsToDefault();
                    }
                }
            });
        }


        void SetTabsToDefault()
        {
            foreach (var playerFrame in playerViewInGames)
            {
                setPlayerFrameTabToDefault(playerFrame);
            }

            Frame.Map.Uri = new Uri("pack://application:,,,/ThunderHawk;component/Images/Maps/default.jpg");
        }

        void setPlayerFrameTabToDefault(PlayerFrameInGame playerFrame)
        {
            playerFrame.Name.Text = "";
            playerFrame.Race.Value = Race.unknown;
            playerFrame.Rating.Text = "";
            playerFrame.LoadBackground.BackgroundColor = "#bdbebd";
            playerFrame.LoadBackground.BackgroundOpacity = 0;
            playerFrame.Name.Text = "";
            playerFrame.Name.Text = "";
        }

        void ShowGame()
        {
            var inGamePlayers = CoreContext.InGameService.inGamePlayers;

            switch (CoreContext.InGameService.inGamePlayers.Length)
            {
                case 2:
                    setPlayerInGameToPlayerTab(playerViewInGames[0], inGamePlayers[0]);
                    setPlayerFrameTabToDefault(playerViewInGames[1]);
                    setPlayerFrameTabToDefault(playerViewInGames[2]);
                    setPlayerFrameTabToDefault(playerViewInGames[3]);
                    setPlayerInGameToPlayerTab(playerViewInGames[4], inGamePlayers[1]);
                    setPlayerFrameTabToDefault(playerViewInGames[5]);
                    setPlayerFrameTabToDefault(playerViewInGames[6]);
                    setPlayerFrameTabToDefault(playerViewInGames[7]);
                    break;
                case 4:
                    setPlayerInGameToPlayerTab(playerViewInGames[0], inGamePlayers[0]);
                    setPlayerInGameToPlayerTab(playerViewInGames[1], inGamePlayers[1]);
                    setPlayerFrameTabToDefault(playerViewInGames[2]);
                    setPlayerFrameTabToDefault(playerViewInGames[3]);
                    setPlayerInGameToPlayerTab(playerViewInGames[4], inGamePlayers[2]);
                    setPlayerInGameToPlayerTab(playerViewInGames[5], inGamePlayers[3]);
                    setPlayerFrameTabToDefault(playerViewInGames[6]);
                    setPlayerFrameTabToDefault(playerViewInGames[7]);
                    break;
                case 6:
                    setPlayerInGameToPlayerTab(playerViewInGames[0], inGamePlayers[0]);
                    setPlayerInGameToPlayerTab(playerViewInGames[1], inGamePlayers[1]);
                    setPlayerInGameToPlayerTab(playerViewInGames[2], inGamePlayers[2]);
                    setPlayerFrameTabToDefault(playerViewInGames[3]);
                    setPlayerInGameToPlayerTab(playerViewInGames[4], inGamePlayers[3]);
                    setPlayerInGameToPlayerTab(playerViewInGames[5], inGamePlayers[4]);
                    setPlayerInGameToPlayerTab(playerViewInGames[6], inGamePlayers[5]);
                    setPlayerFrameTabToDefault(playerViewInGames[7]);
                    break;
                case 8:
                default:
                    for (int i = 0; i < inGamePlayers.Length; i++)
                        setPlayerInGameToPlayerTab(playerViewInGames[i], inGamePlayers[i]);
                    break;
            }
            
            if (Frame.Map != null && CoreContext.ResourcesService.HasImageWithName(CoreContext.InGameService.inGameMap))
            {
                Frame.Map.Uri =
                    new Uri(
                        $"pack://application:,,,/ThunderHawk;component/Images/Maps/{CoreContext.InGameService.inGameMap?.ToLowerInvariant()}.jpg");
            }
            else
            {
                Frame.Map.Uri = new Uri("pack://application:,,,/ThunderHawk;component/Images/Maps/default.jpg");
            }
        }

        private void setPlayerInGameToPlayerTab(PlayerFrameInGame playerInGameTab, InGamePlayer inGamePlayer)
        {
            playerInGameTab.Name.Text = inGamePlayer.Name;
            playerInGameTab.Race.Value = inGamePlayer.Race;
            // team 0 is not human-readable, so team increment on 1
            playerInGameTab.Rating.Text = "Team " + (inGamePlayer.Team + 1) + " - " + inGamePlayer.Mmr;
            if (inGamePlayer.IsLoadComplete)
            {
                playerInGameTab.LoadBackground.BackgroundColor = "#009900";
                playerInGameTab.LoadBackground.BackgroundOpacity = 0.2;
            }
            else
            {
                playerInGameTab.LoadBackground.BackgroundColor = "#bdbebd";
                playerInGameTab.LoadBackground.BackgroundOpacity = 0;
            }
        }

        protected override void OnUnbind()
        {
            _timer?.Dispose();
            base.OnUnbind();
        }
    }
}