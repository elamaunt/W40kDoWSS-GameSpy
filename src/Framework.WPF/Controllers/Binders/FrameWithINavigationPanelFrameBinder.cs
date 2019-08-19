using System;
using System.Runtime.CompilerServices;
using System.Windows.Navigation;

namespace Framework.WPF
{
    public class FrameWithINavigationPanelFrameBinder : BindingController<System.Windows.Controls.Frame, INavigationPanelFrame>
    {
        readonly ConditionalWeakTable<IBindableView, ViewModel> _viewmodelsHistoryCache = new ConditionalWeakTable<IBindableView, ViewModel>();

        IBindableView _currentView;
        protected override void OnBind()
        {
            View.JournalOwnership = JournalOwnership.OwnsJournal;

            SubscribeOnPropertyChanged(Frame, nameof(INavigationPanelFrame.CurrentContentViewModel), UpdateFrameContent);
            UpdateFrameContent();

            Frame.GoBack = View.GoBack;
            Frame.GoForward = View.GoForward;

            View.Navigated += OnNavigated;
        }

        void OnNavigated(object sender, NavigationEventArgs e)
        {
            var view = e.Content as IBindableView;

            _currentView = view;

            if (view != null)
            {
                if (view.ViewModel != null)
                {
                    _viewmodelsHistoryCache.Remove(view);
                    _viewmodelsHistoryCache.Add(view, view.ViewModel);
                }
                else
                {
                    if (_viewmodelsHistoryCache.TryGetValue(view, out ViewModel viewModel))
                        view.ViewModel = viewModel;
                }

                if (Frame.CurrentContentViewModel != view.ViewModel)
                    Frame.CurrentContentViewModel = view.ViewModel;
            }
            
            Frame.CanGoBack = View.CanGoBack;
            Frame.CanGoForward = View.CanGoForward;
        }

        void UpdateFrameContent()
        {
            var model = Frame.CurrentContentViewModel;

            if (model == null)
            {
                if (_currentView != null)
                    _currentView.ViewModel = null;

                View.Content = null;
                return;
            }

            if (_currentView?.ViewModel == model)
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
