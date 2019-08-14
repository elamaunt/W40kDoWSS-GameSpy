using Framework;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ThunderHawk
{
    public class TabControlWithListFrameBinder : BindingController<TabControl, IListFrame>
    {
        readonly List<IBindableView> _bindedElements = new List<IBindableView>();

        protected override void OnBind()
        {
            SubscribeOnPropertyChanged(Frame, nameof(IListFrame.DataSource), OnDataSourceChanged);
            OnDataSourceChanged();
        }

        private void OnDataSourceChanged()
        {
            View.Items.Clear();
            foreach (ViewModel item in Frame.DataSource)
                View.Items.Add(CreateTabItem(item));
        }

        private TabItem CreateTabItem(ViewModel model)
        {
            var item = new TabItem();

            FrameBinder.Bind(item, model);

            var view = (IBindableView)Service<IViewFactory>.Get().CreateView(model.GetPrefix(), model.GetViewStyle());
            view.ViewModel = model;
            item.Content = view;
            
            return item;
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
