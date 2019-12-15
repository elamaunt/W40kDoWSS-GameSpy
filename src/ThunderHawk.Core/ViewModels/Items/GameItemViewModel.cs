using System;
using System.Linq;
using Framework;
using SharedServices;

namespace ThunderHawk.Core
{
    public class GameItemViewModel : ItemViewModel
    {
        public TextFrame UploadedName { get; } = new TextFrame();
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

        public GameItemViewModel(GameInfo game)
        {
            Game = game;

            Type.Text = ToStringValue(game);

            if (game.Map != null && CoreContext.ResourcesService.HasImageWithName(game.Map))
            {
                Map.Uri = new Uri($"pack://application:,,,/ThunderHawk;component/Images/Maps/{game.Map?.ToLowerInvariant()}.jpg");
            }
            else
            {
                Map.Uri = new Uri("pack://application:,,,/ThunderHawk;component/Images/Maps/default.jpg");
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

        void Setup(PlayerFrame frame, PlayerInfo player)
        {
            if (player == null)
            {
                frame.Visible = false;
            }
            else
            {
                frame.Visible = true;
                frame.Name.Text =  $"{player.Name}";
                frame.Race.Value = player.Race;
                frame.Rating.Text = $"({player.FinalState.ToString().Take(3).Select(x => x.ToString().ToUpperInvariant()).Aggregate((x,y) => x+y)} {player.Rating}{WithSign(player.RatingDelta)})";
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
            switch (game.Type)
            {
                case GameType._1v1: return  $"1vs1   /   {game.Map}";
                case GameType._2v2: return $"2vs2   /   {game.Map}";
                case GameType._3v3_4v4:
                    {
                        if (game.Players.Length == 8)
                            return $"4vs4   /   {game.Map}";
                        return $"3vs3   /   {game.Map}";
                    }
                default: return $"non-standard   /   {game.Map}";
            }
        }
    }
}
