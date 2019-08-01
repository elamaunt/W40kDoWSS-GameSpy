
using SteamSpy.StaticClasses;
using SteamSpy.StaticClasses.DataKeepers;
using SteamSpy.WPFHelpClasses;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SteamSpy.ViewModels.PageViewModels
{
    public class MainPageViewModel : IPageViewModel, INotifyPropertyChanged
    {
        #region PropertyChanged (You don't need to edit this)
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public string PageName { get; set; }

        public void SetPageName()
        {
            PageName = LangService.GetString("MainPage");
        }

        private ICommand launchGameCommand;
        public ICommand LaunchGameCommand
        {
            get
            {
                if (launchGameCommand == null)
                {
                    launchGameCommand = new RelayCommand(
                        obj => SoulstormExtensions.LaunchGame(),
                        obj => SoulstormExtensions.CanLaunchGame());

                }
                return launchGameCommand;
            }
        }

        public string GameStateText { get; private set; } = "";
        public string GameStateImage { get; private set; } = "";

        private void UpdateGameState(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.Success:
                    //GameStateText = LangService.GetString("GameStateSuccess");
                    //GameStateImage = "/Images/success-mark.png";
                    GameStateText = ""; // No show info when all is ok
                    GameStateImage = "";
                    break;
                case GameState.Warning:
                    GameStateText = LangService.GetString("GameStateWarning");
                    GameStateImage = "/Images/warning-mark.png";
                    break;
                case GameState.Error:
                    GameStateText = LangService.GetString("GameStateError");
                    GameStateImage = "/Images/error-mark.png";
                    break;
                case GameState.CantRun:
                    GameStateText = LangService.GetString("GameStateCantRun");
                    GameStateImage = "/Images/cantrun-mark.png";
                    break;
            }
        }

        private void UpdateGameStateByPath()
        {
            if (SoulstormExtensions.IsPathFound())
                UpdateGameState(GameState.Success);
            else
                UpdateGameState(GameState.CantRun);
        }

        public MainPageViewModel()
        {
            //RunTimeData.OnPathUpdated += UpdateGameStateByPath;
            UpdateGameStateByPath();
        }
    }
}
