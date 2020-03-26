using Framework;
using SharedServices;
using System;
using System.Linq;

namespace ThunderHawk.Core
{
    public class GameItemViewModel : ItemViewModel
    {
        public TextFrame UploadedName { get; } = new TextFrame();
        public TextFrame Date { get; } = new TextFrame();
        public TextFrame Type { get; } = new TextFrame(); 
        public PlayerFrame Player0 { get; } = new PlayerFrame();
        public PlayerFrame Player1 { get; } = new PlayerFrame();
        public PlayerFrame Player2 { get; } = new PlayerFrame();
        public PlayerFrame Player3 { get; } = new PlayerFrame();
        public PlayerFrame Player4 { get; } = new PlayerFrame();
        public PlayerFrame Player5 { get; } = new PlayerFrame();
        public PlayerFrame Player6 { get; } = new PlayerFrame();
        public PlayerFrame Player7 { get; } = new PlayerFrame();

        public UriFrame Map { get; } = new UriFrame();

        public GameInfo Game { get; }
        public ActionFrame Download { get; } = new ActionFrame();
        public TextFrame Downloaded { get; } = new TextFrame();

        public GameItemViewModel(GameInfo game)
        {
            Downloaded.Text = "In playback";
            Downloaded.Visible = false;

            Game = game;

            Date.Text = ToDateValue(game.PlayedDate);
            Type.Text = ToStringValue(game);

            if (game.Map != null && CoreContext.ResourcesService.HasImageWithName(game.Map))
            {
                Map.Uri = new Uri($"pack://application:,,,/ThunderHawk;component/Resources/Images/Maps/{game.Map?.ToLowerInvariant()}.jpg");
            }
            else
            {
                Map.Uri = new Uri("pack://application:,,,/ThunderHawk;component/Resources/Images/Maps/default.jpg");
            }

            switch (game.Type)
            {
                case GameType.Unknown:
                    Setup(Player0, game.Players.ElementAtOrDefault(0));
                    Setup(Player1, game.Players.ElementAtOrDefault(1));
                    Setup(Player2, game.Players.ElementAtOrDefault(2));
                    Setup(Player3, game.Players.ElementAtOrDefault(4));
                    Setup(Player4, game.Players.ElementAtOrDefault(5));
                    Setup(Player5, game.Players.ElementAtOrDefault(6));
                    Setup(Player6, game.Players.ElementAtOrDefault(7));
                    Setup(Player7, game.Players.ElementAtOrDefault(8));
                    break;
                case GameType._1v1:
                    Setup(Player0, game.Players.ElementAtOrDefault(0));
                    Setup(Player1, null);
                    Setup(Player2, null);
                    Setup(Player3, null);
                    Setup(Player4, game.Players.ElementAtOrDefault(1));
                    Setup(Player5, null);
                    Setup(Player6, null);
                    Setup(Player7, null);
                    break;
                case GameType._2v2:
                    Setup(Player0, game.Players.ElementAtOrDefault(0));
                    Setup(Player1, game.Players.ElementAtOrDefault(1));
                    Setup(Player2, null);
                    Setup(Player3, null);
                    Setup(Player4, game.Players.ElementAtOrDefault(2));
                    Setup(Player5, game.Players.ElementAtOrDefault(3));
                    Setup(Player6, null);
                    Setup(Player7, null);

                    break;
                case GameType._3v3_4v4:
                    if (game.Players.Length == 6)
                    {
                        Setup(Player0, game.Players.ElementAtOrDefault(0));
                        Setup(Player1, game.Players.ElementAtOrDefault(1));
                        Setup(Player2, game.Players.ElementAtOrDefault(2));
                        Setup(Player3, null);
                        Setup(Player4, game.Players.ElementAtOrDefault(3));
                        Setup(Player5, game.Players.ElementAtOrDefault(4));
                        Setup(Player6, game.Players.ElementAtOrDefault(5));
                        Setup(Player7, null);
                    }
                    else
                    {
                        Setup(Player0, game.Players.ElementAtOrDefault(0));
                        Setup(Player1, game.Players.ElementAtOrDefault(1));
                        Setup(Player2, game.Players.ElementAtOrDefault(2));
                        Setup(Player3, game.Players.ElementAtOrDefault(4));
                        Setup(Player4, game.Players.ElementAtOrDefault(5));
                        Setup(Player5, game.Players.ElementAtOrDefault(6));
                        Setup(Player6, game.Players.ElementAtOrDefault(7));
                        Setup(Player7, game.Players.ElementAtOrDefault(8));
                    }
                    break;
                default:
                    Setup(Player0, game.Players.ElementAtOrDefault(0));
                    Setup(Player1, game.Players.ElementAtOrDefault(1));
                    Setup(Player2, game.Players.ElementAtOrDefault(2));
                    Setup(Player3, game.Players.ElementAtOrDefault(4));
                    Setup(Player4, game.Players.ElementAtOrDefault(5));
                    Setup(Player5, game.Players.ElementAtOrDefault(6));
                    Setup(Player6, game.Players.ElementAtOrDefault(7));
                    Setup(Player7, game.Players.ElementAtOrDefault(8));
                    break;
            }
        }

        public string ToDateValue(DateTime playedDate)
        {
            var utcNow = DateTime.Now;

            var span = utcNow - playedDate.ToLocalTime();

            if (span.TotalSeconds < 10)
                return "Some seconds ago";

            if (span.TotalSeconds < 60)
                return $"{(int)span.TotalSeconds} seconds ago";

            if (span.TotalMinutes < 2)
                return "Minute ago";

            if (span.TotalMinutes < 60)
                return $"{(int)span.TotalMinutes} minutes ago";

            if (span.TotalHours < 24)
                return $"{(int)span.TotalHours} hours ago";

            if (span.TotalDays < 7)
                return $"{(int)span.TotalDays} days ago";

            if (playedDate == default(DateTime))
                return "Undefined";

            return playedDate.ToShortDateString();
        }

        void Setup(PlayerFrame frame, PlayerInfo player)
        {
            if (player == null)
            {
                frame.Visible = false;
                frame.Race.Visible = false;
            }
            else
            {
                frame.Visible = true;
                frame.Race.Visible = true;
                frame.Name.Text =  $"{player.Name}";
                frame.Race.Value = player.Race;
                frame.Rating.Text = $"{player.FinalState.ToString().Take(3).Select(x => x.ToString().ToUpperInvariant()).Aggregate((x,y) => x+y)} {Math.Max(1000,player.Rating)}{WithSign(player.RatingDelta)}";
                frame.Team.Value = player.Team;
            }
        }

        private string WithSign(long ratingDelta)
        {
            if (ratingDelta == 0)
                return "";

            if (ratingDelta > 0)
                return " +"+ ratingDelta;

            return " " + ratingDelta;
        }

        string ToStringValue(GameInfo game)
        {
            var ranked = game.IsRateGame ? "ranked" : "unranked";

            switch (game.Type)
            {
                case GameType._1v1: return  $"1vs1 {ranked}   /   {game.ModName}   /   {game.Map?.ToUpperInvariant()}   /   {ToTimeString(game.Duration)}";
                case GameType._2v2: return $"2vs2 {ranked}   /   {game.ModName}   /   {game.Map?.ToUpperInvariant()}   /   {ToTimeString(game.Duration)}";
                case GameType._3v3_4v4:
                    {
                        if (game.Players.Length == 8)
                            return $"4vs4 {ranked}   /   {game.ModName}   /   {game.Map?.ToUpperInvariant()}   /   {ToTimeString(game.Duration)}";
                        return $"3vs3 {ranked}   /   {game.ModName}   /   {game.Map?.ToUpperInvariant()}   /   {ToTimeString(game.Duration)}";
                    }
                default: return $"non-standard   /   {game.ModName}   /   {game.Map?.ToUpperInvariant()}   /   {ToTimeString(game.Duration)}";
            }
        }

        private string ToTimeString(long duration)
        {
            var span = new TimeSpan(0,0,0, (int)duration);

            if (span.TotalHours >= 1)
                return span.ToString(@"h\:mm\:ss");

            return span.ToString(@"mm\:ss");
        }
    }
}
