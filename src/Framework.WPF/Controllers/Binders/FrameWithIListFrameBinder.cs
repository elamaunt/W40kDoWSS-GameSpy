namespace Framework.WPF
{
    public class FrameWithIListFrameBinder : BindingController<System.Windows.Controls.Frame, IListFrame>
    {
        IBindableView _currentView;

        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(IListFrame.SelectedItemsIndexes), OnSelectedItemsChanged);
            OnSelectedItemsChanged();
        }

        void OnSelectedItemsChanged()
        {
            var indexes = Frame.SelectedItemsIndexes;

            if (indexes.IsNullOrEmpty())
            {
                if (_currentView == null)
                    return;
                
                _currentView.ViewModel = null;
                View.Content = null;
                return;
            }

            var model = Frame.GetItemAtIndex(indexes[0]);
            var view = _currentView = (IBindableView)Service<IViewFactory>.Get().CreateView(model.GetPrefix(), model.GetViewStyle());
            view.ViewModel = model;
            View.Content = view;
        }
    }
}
