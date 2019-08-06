using SteamSpy.StaticClasses;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace SteamSpy.ViewModels
{
    class SettingsViewModel : INotifyPropertyChanged
    {
        #region PropertyChanged (You don't need to edit this)
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool DisableInGameFog
        {
            get => Options.DisableFog;
            set
            {
                Options.DisableFog = value;
            }
        }
    }
}
