using SteamSpy.Extensions;
using SteamSpy.Services;
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
                        obj => !SoulstormExtensions.IsGameRunning());

                }
                return launchGameCommand;
            }
        }
    }
}
