using System;

namespace Framework
{
    public interface INavigationPanelFrame : IControlFrame
    {
        ViewModel CurrentContentViewModel { get; set; }
        bool CanGoBack { get; set; }
        bool CanGoForward { get; set; }

        Action GoBack { get; set; }
        Action GoForward { get; set; }
    }
}
