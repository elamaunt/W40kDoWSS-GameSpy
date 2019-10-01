using System;
using System.Collections.Specialized;
using System.Windows.Controls;

namespace Framework.WPF
{
    public class ListViewWithIListFrameBinder : BindingController<ListView, IListFrame>, IScrollManager
    {
        INotifyCollectionChanged _previousSource;

        protected override void OnBind()
        {
            Frame.SetExtension<IScrollManager>(this);

            View.ItemTemplateSelector = ViewModelTemplateSelector.Instance;
            SubscribeOnPropertyChanged(Frame, nameof(IListFrame.DataSource), OnDataSourceChanged);
            OnDataSourceChanged();
        }

        void OnDataSourceChanged()
        {
            if (_previousSource != null)
                _previousSource.CollectionChanged -= OnCollectionChanged;

            _previousSource = Frame.DataSource as INotifyCollectionChanged;

            if (_previousSource != null)
                _previousSource.CollectionChanged += OnCollectionChanged;

            View.Items.Clear();

            foreach (ViewModel item in Frame.DataSource)
                View.Items.Add(item);
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                for (int i = 0; i < e.OldItems.Count; i++)
                    View.Items.Remove(e.OldItems[i]);

            if (e.NewItems != null)
                for (int i = 0; i < e.NewItems.Count; i++)
                    View.Items.Add(e.NewItems[i]);

            /* switch (e.Action)
             {
                 case NotifyCollectionChangedAction.Add:
                     break;
                 case NotifyCollectionChangedAction.Remove:
                     break;
                 case NotifyCollectionChangedAction.Replace:
                     break;
                 case NotifyCollectionChangedAction.Move:
                     break;
                 case NotifyCollectionChangedAction.Reset:
                     break;
                 default:
                     break;
             }*/
        }

        float IScrollManager.ScrollOffset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        float IScrollManager.MaxScrollOffset => throw new NotImplementedException();

        void IScrollManager.ScrollToOffsetAnimated(int offset, float duration)
        {
            throw new NotImplementedException();
        }

        void IScrollManager.ScrollToItem(ItemViewModel item, ScrollPosition position = ScrollPosition.None, bool animated = true)
        {
            View.ScrollIntoView(item);
        }

        void IScrollManager.ScrollToItem(int index, ScrollPosition position = ScrollPosition.None, bool animated = true)
        {
            throw new NotImplementedException();
        }

        bool IScrollManager.IsItemVisible(ItemViewModel item)
        {
            throw new NotImplementedException();
        }

        bool IScrollManager.IsScrolledToEnd()
        {
            throw new NotImplementedException();
        }

        bool IScrollManager.IsScrolledToStart()
        {
            throw new NotImplementedException();
        }

        object[] IScrollManager.GetVisibleItems()
        {
            throw new NotImplementedException();
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
