using System;

namespace Framework
{
    public class NavigationPanelFrame : ControlFrame, INavigationPanelFrame
    {
        ViewModel _currentContentViewModel;
        bool _canGoBack;
        bool _canGoForward;

        public ViewModel CurrentContentViewModel
        {
            get => _currentContentViewModel;
            set
            {
                if (_currentContentViewModel == value)
                    return;
                _currentContentViewModel = value;
                FirePropertyChanged(nameof(CurrentContentViewModel));
            }
        }

        public bool CanGoBack
        {
            get => _canGoBack;
            set
            {
                if (_canGoBack == value)
                    return;
                _canGoBack = value;
                FirePropertyChanged(nameof(CanGoBack));
            }
        }

        public bool CanGoForward
        {
            get => _canGoForward;
            set
            {
                if (_canGoForward == value)
                    return;
                _canGoForward = value;
                FirePropertyChanged(nameof(CanGoForward));
            }
        }

        public Action GoBack
        {
            get;
            set;
        }

        public Action GoForward
        {
            get;
            set;
        }
    }
}
