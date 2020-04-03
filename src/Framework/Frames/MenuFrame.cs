using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Framework
{
    public class MenuFrame : ControlFrame, IMenuFrame
    {
        ObservableCollection<IMenuItemFrame> _menuItemsSource;
        public ObservableCollection<IMenuItemFrame> MenuItems
        {
            get => _menuItemsSource ?? new ObservableCollection<IMenuItemFrame>();
            set
            {
                if (_menuItemsSource != null)
                    _menuItemsSource.CollectionChanged -= OnCollectionChanged;

                _menuItemsSource = value;

                if (_menuItemsSource != null)
                    _menuItemsSource.CollectionChanged += OnCollectionChanged;

                FirePropertyChanged(nameof(MenuItems));
            }
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FirePropertyChanged(nameof(ItemsCount));
        }

        IEnumerable<IMenuItemFrame> IMenuFrame.MenuItems => MenuItems;

        public int ItemsCount => _menuItemsSource?.Count ?? 0;
    }
}
