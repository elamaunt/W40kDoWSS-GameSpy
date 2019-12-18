using Framework;
using SharedServices;
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

            CoreContext.MasterServer.UserNameChanged += OnUserChanged;
            CoreContext.MasterServer.UserStatsChanged += OnUserStatsChanged;

            UpdateRatingLabel();

            /*Frame.LastGames.DataSource.Add(new GameItemViewModel(new GameInfo()
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
            }));*/
        }

        void OnUserStatsChanged(StatsChangesInfo changes)
        {
            if (changes.User.IsUser)
                UpdateRatingLabel();
        }

        void OnUserChanged(UserInfo user, long? profileId, string previousName, string name)
        {
            if (user.IsUser)
                UpdateRatingLabel();
        }

        void UpdateRatingLabel()
        {
            RunOnUIThread(() =>
            {
                var user = CoreContext.MasterServer.CurrentProfile;

                if (user != null)
                    Frame.Rating.Text = $"{user.UIName}    {user.Wins}/{user.Games}  ({(((float)user.Wins) / user.Games)?.ToString("P")})     1v1: {user.Score1v1}   2v2: {user.Score2v2}   3v3/4v4: {user.Score3v3}";
            });
        }

        void OnLastGamesLoaded(GameInfo[] games)
        {
            var source = games.Select(x => new GameItemViewModel(x)).ToObservableCollection();

            RunOnUIThread(() =>
            {
                Frame.LastGames.DataSource = source;
            });
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
            CoreContext.MasterServer.UserNameChanged -= OnUserChanged;
            CoreContext.MasterServer.UserStatsChanged -= OnUserStatsChanged;
            CoreContext.MasterServer.LastGamesLoaded -= OnLastGamesLoaded;
            CoreContext.MasterServer.PlayersTopLoaded -= OnPlayersTopLoaded;
            base.OnUnbind();
        }

    }
}
