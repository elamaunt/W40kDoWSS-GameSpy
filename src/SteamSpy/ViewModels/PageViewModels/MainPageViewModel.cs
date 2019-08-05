
using SteamSpy.Models;
using SteamSpy.Providers;
using SteamSpy.StaticClasses;
using SteamSpy.StaticClasses.DataKeepers;
using SteamSpy.StaticClasses.Services;
using SteamSpy.WPFHelpClasses;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

        private INewsProvider newsProvider;
        public ObservableCollection<NewsModel> NewsList { get; set; } = new ObservableCollection<NewsModel>();

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
            if (Config.TESTING_BEHAVIOUR)
            {
                newsProvider = new TestNewsProvider();
            }
            else
            {
                newsProvider = new ServerNewsProvider();
            }
            //RunTimeData.OnPathUpdated += UpdateGameStateByPath;
            UpdateGameStateByPath();

            Task.Run(() => ReceiveNews());
        }

        private void ReceiveNews()
        {
            while (true)
            {
                try
                {
                    var newData = newsProvider.GetNews().ToList();
                    if (newData != null)
                    {
                        DispatchService.Invoke(new Action(() =>
                        {
                            NewsList.Clear();
                            // Maximum 3 news
                            if (newData.Count > 3)
                                newData.RemoveRange(3, newData.Count - 3);
                            foreach (var news in newData)
                            {
                                NewsList.Add(new NewsModel(news));
                            }
                        }));
                    }
                    Task.Delay(1000 * 10); // Check it every 10s
                }
                catch (Exception ex)
                {
                    //TODO: LOGGER IS HERE
                    Task.Delay(1000 * 10 * 2);
                }
            }
        }
    }
}
