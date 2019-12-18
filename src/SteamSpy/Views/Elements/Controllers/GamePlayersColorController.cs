using Framework;
using System.Windows.Media;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class GamePlayersColorController : BindingController<Element_Game, GameItemViewModel>
    {
        static SolidColorBrush _teamEmptyBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));

        static SolidColorBrush _team0Brush = new SolidColorBrush(Color.FromRgb(200,70,70));
        static SolidColorBrush _team1Brush = new SolidColorBrush(Color.FromRgb(70, 70, 200));
        static SolidColorBrush _team2Brush = new SolidColorBrush(Color.FromRgb(70, 200, 70));
        static SolidColorBrush _team3Brush = new SolidColorBrush(Color.FromRgb(70,200, 200));
        static SolidColorBrush _team4Brush = new SolidColorBrush(Color.FromRgb(200, 200,70));
        static SolidColorBrush _team5Brush = new SolidColorBrush(Color.FromRgb(200, 70, 200));
        static SolidColorBrush _team6Brush = new SolidColorBrush(Color.FromRgb(100,200,175));
        static SolidColorBrush _team7Brush = new SolidColorBrush(Color.FromRgb(175, 100, 200));

        protected override bool NeedsUnbind => false;

        protected override void OnBind()
        {
            View.Player0.Foreground = GetColorOfTeam(Frame.Player0.Team.Value);
            View.Player1.Foreground = GetColorOfTeam(Frame.Player1.Team.Value);
            View.Player2.Foreground = GetColorOfTeam(Frame.Player2.Team.Value);
            View.Player3.Foreground = GetColorOfTeam(Frame.Player3.Team.Value);
            View.Player4.Foreground = GetColorOfTeam(Frame.Player4.Team.Value);
            View.Player5.Foreground = GetColorOfTeam(Frame.Player5.Team.Value);
            View.Player6.Foreground = GetColorOfTeam(Frame.Player6.Team.Value);
            View.Player7.Foreground = GetColorOfTeam(Frame.Player7.Team.Value);
        }

        Brush GetColorOfTeam(long value)
        {
            switch (value)
            {
                case 0: return _team0Brush;
                case 1: return _team1Brush;
                case 2: return _team2Brush;
                case 3: return _team3Brush;
                case 4: return _team4Brush;
                case 5: return _team5Brush;
                case 6: return _team6Brush;
                case 7: return _team7Brush;
                default: return _teamEmptyBrush;
            }
        }
    }
}
