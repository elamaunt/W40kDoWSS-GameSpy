using Framework;

namespace ThunderHawk.Core
{
    public class MainWindowNavigationController : FrameController<MainWindowViewModel>
    {
        protected override void OnBind()
        {
            foreach (var item in Frame.Pages.DataSource)
                item.ViewModel.TitleButton.Action = () => OnTabClicked(item);

            OnTabClicked(Frame.Pages.SelectedItem);

            SubscribeOnPropertyChanged(Frame.NavigationPanel, nameof(INavigationPanelFrame.CanGoBack), OnCanGoBackChanged);
            SubscribeOnPropertyChanged(Frame.NavigationPanel, nameof(INavigationPanelFrame.CanGoForward), OnCanGoForwardChanged);
            SubscribeOnPropertyChanged(Frame.NavigationPanel, nameof(INavigationPanelFrame.CurrentContentViewModel), OnCurrentViewModelChanged);

            OnCurrentViewModelChanged();
            OnCanGoBackChanged();
            OnCanGoForwardChanged();

            Frame.GoBack.Action = () => Frame.NavigationPanel.GoBack?.Invoke();
            Frame.GoForward.Action = () => Frame.NavigationPanel.GoForward?.Invoke();
        }

        void OnTabClicked(PageTabViewModel model)
        {
            Frame.Pages.SelectedItem = model;
            Frame.NavigationPanel.CurrentContentViewModel = model?.ViewModel;
        }

        void OnCurrentViewModelChanged()
        {
            RunOnUIThread(() =>
            {
                var model = Frame.NavigationPanel.CurrentContentViewModel;

                if (Frame.NavigationPanel.CurrentContentViewModel == Frame.ChatViewModel)
                {
                    Frame.ChatTabViewModel.NewMessagesCounter.Visible = false;
                    Frame.ChatTabViewModel.NewMessagesCounter.Value = 0;
                }

                foreach (var item in Frame.Pages.DataSource)
                {
                    item.ViewModel.TitleButton.IsChecked = model == item.ViewModel;
                    item.ViewModel.TitleButton.Enabled = model != item.ViewModel;
                }
            });
        }

        void OnCanGoBackChanged()
        {
            RunOnUIThread(() =>
            {
                Frame.GoBack.Visible = Frame.NavigationPanel.CanGoBack;
            });
        }

        void OnCanGoForwardChanged()
        {
            RunOnUIThread(() =>
            {
                Frame.GoForward.Visible = Frame.NavigationPanel.CanGoForward;
            });
        }
    }
}
