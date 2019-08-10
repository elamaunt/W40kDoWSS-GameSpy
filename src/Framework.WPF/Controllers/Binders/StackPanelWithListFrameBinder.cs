using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Framework.WPF
{
    public class StackPanelWithListFrameBinder : BindingController<StackPanel, IListFrame>
    {
        readonly List<IBindableView> _bindedElements = new List<IBindableView>();

        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(IListFrame.DataSource), OnDataSourceChanged);
            OnDataSourceChanged();
        }

        private void OnDataSourceChanged()
        {
            View.Children.Clear();
            foreach (ViewModel item in Frame.DataSource)
                View.Children.Add(CreateItemCell(item));
        }

        private UIElement CreateItemCell(ViewModel model)
        {
            var view = (IBindableView)Service<IViewFactory>.Get().CreateView(model.GetPrefix(), model.GetViewStyle());
            view.ViewModel = model;
            _bindedElements.Add(view);
            return (UIElement)view;
        }

        protected override void OnUnbind()
        {
            for (int i = 0; i < _bindedElements.Count; i++)
                _bindedElements[i].ViewModel = null;

            _bindedElements.Clear();

            base.OnUnbind();
        }
    }
}
