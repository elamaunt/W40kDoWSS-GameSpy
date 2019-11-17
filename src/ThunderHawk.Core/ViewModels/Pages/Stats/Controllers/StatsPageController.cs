using Framework;
using SharedServices;
using System;
using System.Linq;

namespace ThunderHawk.Core
{
    public class StatsPageController : FrameController<StatsPageViewModel>
    {
        protected override void OnBind()
        {
            if (CoreContext.MasterServer.IsLastGamesLoaded)
            {
                var games = CoreContext.MasterServer.LastGames;
                OnLastGamesLoaded(games);
            }
            else
            {
                CoreContext.MasterServer.LastGamesLoaded += OnLastGamesLoaded;
                CoreContext.MasterServer.RequestLastGames();
            }

            if (CoreContext.MasterServer.IsPlayersTopLoaded)
            {
                var top = CoreContext.MasterServer.PlayersTop;
                OnPlayersTopLoaded(top, 0, top.Length);
            }
            else
            {
                CoreContext.MasterServer.PlayersTopLoaded += OnPlayersTopLoaded;
                CoreContext.MasterServer.RequestPlayersTop(0, 10);
            }

            Frame.LastGames.DataSource.Add(new GameItemViewModel(new GameInfo()
            {
                IsRateGame = true,
                ModName = "thunderhawk",
                ModVersion = "1.0a",
                SessionId = "session",
                Type = GameType._1v1,
                Players = new PlayerInfo[]
                {
                    new PlayerInfo() { Name = "elamaunt", FinalState = PlayerFinalState.Winner, Race = Race.ork_race, Rating = 1200, RatingDelta=16, Team = 0  },
                    new PlayerInfo() { Name = "tester", FinalState = PlayerFinalState.Loser, Race = Race.space_marine_race, Rating = 1260, RatingDelta=-16, Team = 1  },
                }
            }));

            CoreContext.MasterServer.NewGameReceived += OnNewGameReceived;
        }

        void OnNewGameReceived(GameInfo game)
        {
            var vm = new GameItemViewModel(game);

            RunOnUIThread(() =>
            {
                Frame.LastGames.DataSource.Add(vm);
            });
        }

        void OnLastGamesLoaded(GameInfo[] games)
        {
           /* var source = games.Select(x => new GameItemViewModel(x)).ToObservableCollection();

            RunOnUIThread(() =>
            {
                Frame.LastGames.DataSource = source;
            });*/
        }

        void OnPlayersTopLoaded(StatsInfo[] players, int offset, int count)
        {
            var source = players.OfType<StatsInfo>().Select((x, i) => new PlayerItemViewModel(x, i + 1)).ToObservableCollection();

            RunOnUIThread(() =>
            {
                Frame.Top10Players.DataSource = source;
            });
        }

        protected override void OnUnbind()
        {
            CoreContext.MasterServer.NewGameReceived -= OnNewGameReceived;
            CoreContext.MasterServer.LastGamesLoaded -= OnLastGamesLoaded;
            CoreContext.MasterServer.PlayersTopLoaded -= OnPlayersTopLoaded;
            base.OnUnbind();
        }

    }
}
