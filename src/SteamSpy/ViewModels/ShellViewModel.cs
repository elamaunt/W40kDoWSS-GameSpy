using SteamSpy.ViewModels.PageViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SteamSpy.ViewModels
{
    public class ShellViewModel : INotifyPropertyChanged
    {
        #region PropertyChanged (You don't need to edit this)
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private IPageViewModel currentPageViewModel;
        public IPageViewModel CurrentPageViewModel
        {
            get
            {
                return currentPageViewModel;
            }
            set
            {
                if (currentPageViewModel != value)
                {
                    currentPageViewModel = value;
                    OnPropertyChanged();
                }
            }
        }

        private List<IPageViewModel> pageViewModels;
        public List<IPageViewModel> PageViewModels
        {
            get
            {
                if (pageViewModels == null)
                    pageViewModels = new List<IPageViewModel>();

                return pageViewModels;
            }
        }


        private ICommand changePageCommand;
        public ICommand ChangePageCommand
        {
            get
            {
                if (changePageCommand == null)
                {
                    changePageCommand = new RelayCommand(
                        p => ChangeViewModel((IPageViewModel)p),
                        p => p != CurrentPageViewModel);

                }

                return changePageCommand;
            }
        }
        private void SetPageNames()
        {
            foreach (var page in PageViewModels)
            {
                page.SetPageName();
            }
        }

        public ShellViewModel()
        {
            PageViewModels.Add(new MainPageViewModel());
            //PageViewModels.Add(new MainPageViewModel());
            //PageViewModels.Add(new MainPageViewModel());
            //PageViewModels.Add(new MainPageViewModel());
            //PageViewModels.Add(new MainPageViewModel());
            //PageViewModels.Add(new MainPageViewModel());
            //PageViewModels.Add(new MainPageViewModel());
            //PageViewModels.Add(new LobbyPageViewModel());
            SetPageNames();

            CurrentPageViewModel = PageViewModels[0];

        }

        private void ChangeViewModel(IPageViewModel viewModel)
        {
            if (!PageViewModels.Contains(viewModel))
                PageViewModels.Add(viewModel);

            CurrentPageViewModel = PageViewModels
                .FirstOrDefault(vm => vm == viewModel);
        }

    }
}
