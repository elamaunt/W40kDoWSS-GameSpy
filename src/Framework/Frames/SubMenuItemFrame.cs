using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Framework.Frames
{
    public class SubMenuItemFrame : MenuItemFrame, ISubMenuItemFrame
    {
        ObservableCollection<IMenuItemFrame> _innerItemsSource;
        public ObservableCollection<IMenuItemFrame> InnerItems
        {
            get => _innerItemsSource ?? new ObservableCollection<IMenuItemFrame>();
            set
            {
                if (_innerItemsSource != null)
                    _innerItemsSource.CollectionChanged -= OnCollectionChanged;

                _innerItemsSource = value;

                if (_innerItemsSource != null)
                    _innerItemsSource.CollectionChanged += OnCollectionChanged;

                FirePropertyChanged(nameof(InnerItems));
            }
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FirePropertyChanged(nameof(ItemsCount));
        }

        IEnumerable<IMenuItemFrame> ISubMenuItemFrame.InnerItems => InnerItems;

        public int ItemsCount => _innerItemsSource?.Count ?? 0;
    }
}
