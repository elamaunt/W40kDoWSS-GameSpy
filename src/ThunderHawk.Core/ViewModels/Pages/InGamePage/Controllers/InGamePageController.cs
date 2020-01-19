using System;
using System.Threading;
using Framework;
using SharedServices;
using ThunderHawk.Core.Frames;

namespace ThunderHawk.Core
{
    public class InGamePageController : FrameController<InGamePageViewModel>
    {
        Timer _timer;

        PlayerFrameInGame[] playerViewInGames = new PlayerFrameInGame[8];

        protected override void OnBind()
        {
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
                playerFrame.LoadBackground.BackgroundOpacity = 0.2;
            }

            Frame.Change.Action = ShowGame;

            _timer = new Timer(OnTime, null, 1000, 1000);
        }

        void OnTime(object state)
        {
            RunOnUIThread(() =>
            {
                if (CoreContext.InGameService.isGameNow && CoreContext.LaunchService.GameProcess != null)
                {
                    ShowGame();
                    Frame.InfoLabel.Text = "";
                }
                else
                {
                    if (CoreContext.LaunchService.GameProcess == null) {
                        Frame.InfoLabel.Text = "Soulstorm is not running or has been launched in another way";
                    } else Frame.InfoLabel.Text = "Successful subscribe to Soulstorm - waiting for the game start";
                    
                    SetTabsToDefault();
                }

                Frame.ApmLabel.Text = "APM: " + CoreContext.InGameService.apmCurrent;
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
            playerInGameTab.LoadBackground.BackgroundOpacity = 0.2;
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
                playerInGameTab.LoadBackground.BackgroundOpacity = 0.2;
            }
        }

        protected override void OnUnbind()
        {
            _timer?.Dispose();
            base.OnUnbind();
        }
    }
}