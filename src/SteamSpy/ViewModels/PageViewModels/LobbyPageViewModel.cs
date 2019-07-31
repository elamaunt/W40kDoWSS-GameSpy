using SteamSpy.StaticClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SteamSpy.ViewModels.PageViewModels
{
    class LobbyPageViewModel : IPageViewModel, INotifyPropertyChanged
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
            PageName = LangService.GetString("LobbyPage");
        }
    }
}
