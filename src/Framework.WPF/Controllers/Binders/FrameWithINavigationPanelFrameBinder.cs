using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace Framework.WPF
{
    public class FrameWithINavigationPanelFrameBinder : BindingController<System.Windows.Controls.Frame, INavigationPanelFrame>
    {
        IBindableView _currentView;
        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(INavigationPanelFrame.CurrentContentViewModel), UpdateFrameContent);
            UpdateFrameContent();

            Frame.GoBack = View.GoBack;
            Frame.GoForward = View.GoForward;

            View.Navigated += OnNavigated;
        }

        void OnNavigated(object sender, NavigationEventArgs e)
        {
            var view = e.Content as IBindableView;

            if (view != null)
                Frame.CurrentContentViewModel = view.ViewModel;
            //else
            //    Frame.CurrentContentViewModel = null;

            Frame.CanGoBack = View.CanGoBack;
            Frame.CanGoForward = View.CanGoForward;
        }

        void UpdateFrameContent()
        {
            if (_currentView != null)
                _currentView.ViewModel = null;

            var model = Frame.CurrentContentViewModel;

            if (model == null)
                return;

            var view = _currentView = (IBindableView)Service<IViewFactory>.Get().CreateView(model.GetPrefix(), model.GetViewStyle());
            view.ViewModel = model;

            var ext = Frame.GetExtension<ICustomContentPresenter>();
            if (ext != null)
                ext.Present(View, view);
            else
                View.Content = view;
        }

        protected override void OnUnbind()
        {
            View.Navigated -= OnNavigated;
            base.OnUnbind();
        }
    }
}
