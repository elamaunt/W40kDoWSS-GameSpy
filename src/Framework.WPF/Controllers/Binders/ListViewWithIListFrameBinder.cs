using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Framework.WPF
{
    public class ListViewWithIListFrameBinder : BindingController<ListView, IListFrame>
    {
        protected override void OnBind()
        {
            View.ItemTemplateSelector = ViewModelTemplateSelector.Instance;
            SubscribeOnPropertyChanged(Frame, nameof(IListFrame.DataSource), OnDataSourceChanged);
            OnDataSourceChanged();
        }

        private void OnDataSourceChanged()
        {
            View.Items.Clear();

            foreach (ViewModel item in Frame.DataSource)
                View.Items.Add(item);
        }

        /*private FrameworkElement CreateItemCell(ViewModel model, int index)
        {
            var view = (IBindableView)Service<IViewFactory>.Get().CreateView(model.GetPrefix(), model.GetViewStyle());

            view.ViewModel = model;
            return (FrameworkElement)view;
        }*/

        /*protected override void OnUnbind()
        {
            for (int i = 0; i < _bindedElements.Count; i++)
                _bindedElements[i].ViewModel = null;

            _bindedElements.Clear();

            base.OnUnbind();
        }*/
    }
}
