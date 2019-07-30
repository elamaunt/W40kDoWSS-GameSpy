using System.ComponentModel;
using System.Runtime.CompilerServices;

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
    }
}
