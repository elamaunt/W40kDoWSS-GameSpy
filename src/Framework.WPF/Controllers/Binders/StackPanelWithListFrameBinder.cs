using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Framework.WPF
{
    public class StackPanelWithListFrameBinder : BindingController<StackPanel, IListFrame>
    {
        readonly List<IBindableView> _bindedElements = new List<IBindableView>();

        string _itemPrefix;
        string _itemStyle;

        protected override void OnBind()
        {
            _itemPrefix = View.GetItemPrefix();
            _itemStyle = View.GetItemStyle();

            SubscribeOnPropertyChanged(Frame, nameof(IListFrame.DataSource), OnDataSourceChanged);
            OnDataSourceChanged();
        }


        private void OnDataSourceChanged()
        {
            View.Children.Clear();

            int i;

            for (i = 0; i < _bindedElements.Count; i++)
                _bindedElements[i].ViewModel = null;

            _bindedElements.Clear();

            i = 0;

            var ext = Frame.GetExtension<ICustomItemPresenter>();
            foreach (ViewModel item in Frame.DataSource)
            {
                var cell = CreateItemCell(item, i++);
                View.Children.Add(cell);

                if (ext != null)
                    ext.Present(View, cell);
            }
        }

        private FrameworkElement CreateItemCell(ViewModel model, int index)
        {
            var view = (IBindableView)Service<IViewFactory>.Get().CreateView(_itemPrefix ?? model.GetPrefix(), _itemStyle ?? model.GetViewStyle());

            view.ViewModel = model;
            _bindedElements.Add(view);
            return (FrameworkElement)view;
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
