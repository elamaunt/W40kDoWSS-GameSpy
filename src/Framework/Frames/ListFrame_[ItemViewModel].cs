using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Framework
{
    public class ListFrame<ViewModelType> : ControlFrame, IListFrame<ViewModelType>
        where ViewModelType : ViewModel
    {
        public const string _defaultStyle = "Default";

        static readonly int[] _emptySelection = new int[0];
        static readonly List<ViewModelType> _emptyDataSource = new List<ViewModelType>();

        Func<ViewModelType, bool> _filter;
        int[] _selectedItemsIndexes;
        bool _groupingEnabled;

        ObservableCollection<ViewModelType> m_internalDataSource;
        ObservableCollection<ViewModelType> m_visibleDataSource;
        Action<ViewModelType> m_click;
        Action<ViewModelType> m_longClick;

        volatile bool m_blockInternalCollectionEvent;
        volatile bool m_blockVisibleCollectionEvent;

        Dictionary<string, ViewModel> m_categoriesModels;

        public ListFrame()
        {
            m_internalDataSource = new ObservableCollection<ViewModelType>();
            m_internalDataSource.CollectionChanged += OnInternalCollectionChanged;
            UpdateVisibleDataSource();
        }

        public ObservableCollection<ViewModelType> DataSource
        {
            get { return m_visibleDataSource; }
            set
            {
                if (m_internalDataSource == value)
                    return;

                _selectedItemsIndexes = null;

                if (m_internalDataSource != null)
                    m_internalDataSource.CollectionChanged -= OnInternalCollectionChanged;

                m_internalDataSource = value;

                if (m_internalDataSource != null)
                    m_internalDataSource.CollectionChanged += OnInternalCollectionChanged;

                UpdateVisibleDataSource();
                FirePropertyChanged(nameof(DataSource));
                FirePropertyChanged(nameof(SelectedItem));
                FirePropertyChanged(nameof(SelectedItems));
                FirePropertyChanged(nameof(SelectedItemsIndexes));
            }
        }

        public ViewModelType[] SelectedItems
        {
            get
            {
                if (_selectedItemsIndexes == null)
                    return Enumerable.Empty<ViewModelType>() as ViewModelType[];

                return _selectedItemsIndexes.Select(x => SafeDataSource[x]).ToArray();
            }
            set
            {
                if (IsSameSelection(value))
                    return;

                if (value == null || value.Length == 0)
                    _selectedItemsIndexes = null;
                else
                {
                    _selectedItemsIndexes = value.Select(x =>
                    {
                        var index = SafeDataSource.IndexOf(x);
                        if (index < 0)
                            throw new Exception($"Unknown item {x}");
                        return index;
                    }).ToArray();
                }

                FirePropertyChanged(nameof(SelectedItem));
                FirePropertyChanged(nameof(SelectedItems));
                FirePropertyChanged(nameof(SelectedItemsIndexes));
            }
        }

        public ViewModelType SelectedItem
        {
            get
            {
                if (_selectedItemsIndexes.IsNullOrEmpty())
                    return null;

                return SafeDataSource[_selectedItemsIndexes[0]];
            }
            set
            {
                if (_selectedItemsIndexes == null && value == null)
                    return;

                if (SelectedItem == value && _selectedItemsIndexes != null && _selectedItemsIndexes.Length == 1)
                    return;

                _selectedItemsIndexes = value == null ? null : new int[] { SafeDataSource.IndexOf(value) };

                FirePropertyChanged(nameof(SelectedItem));
                FirePropertyChanged(nameof(SelectedItems));
                FirePropertyChanged(nameof(SelectedItemsIndexes));
            }
        }

        public int[] SelectedItemsIndexes
        {
            get
            {
                if (_selectedItemsIndexes == null)
                    return _emptySelection;

                return _selectedItemsIndexes;
            }
            set
            {
                if (IsSameSelection(value))
                    return;

                if (value == null || value.Length == 0)
                    _selectedItemsIndexes = null;
                else
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (value[i] < 0 || value[i] >= SafeDataSource.Count)
                            throw new IndexOutOfRangeException();
                    }
                    _selectedItemsIndexes = value;
                }

                FirePropertyChanged(nameof(SelectedItem));
                FirePropertyChanged(nameof(SelectedItems));
                FirePropertyChanged(nameof(SelectedItemsIndexes));
            }
        }

        public Func<ViewModelType, long> GetItemId { get; set; }
        
        public Func<string, string> GetCategoryViewStyle { get; set; }
        
        public Func<string, string, ItemViewModel> GetCategoryModel { get; set; }

        public Func<ViewModelType, bool> Filter
        {
            get => _filter;
            set
            {
                if (_filter == value)
                    return;

                _filter = value;
                UpdateVisibleDataSource();
                FirePropertyChanged(nameof(Filter));
                FirePropertyChanged(nameof(DataSource));
            }
        }
        
        public bool IsFiltered => Filter != null;
        public int ItemsCount => SafeDataSource.Count;

        public void Refilter()
        {
            UpdateVisibleDataSource();
            FirePropertyChanged(nameof(Filter));
            FirePropertyChanged(nameof(DataSource));
        }

        public Action<ViewModelType> ItemClick
        {
            get => m_click;
            set
            {
                if (m_click == value)
                    return;

                m_click = value;
                FirePropertyChanged(nameof(ItemClick));
            }
        }
        
        public Action<ViewModelType> ItemLongClick
        {
            get => m_longClick;
            set
            {
                if (m_longClick == value)
                    return;

                m_longClick = value;

                FirePropertyChanged(nameof(ItemLongClick));
            }
        }

        public override void Cleanup()
        {
            m_categoriesModels = null;
            m_click = null;
            m_longClick = null;
            GetItemId = null;
            GetCategoryModel = null;
            Filter = null;
            base.Cleanup();
        }

        void UpdateVisibleDataSource()
        {
            if (m_visibleDataSource != null)
                m_visibleDataSource.CollectionChanged -= OnVisibleCollectionChanged;

            if (_filter == null)
            {
                m_visibleDataSource = m_internalDataSource;
                return;
            }

            m_visibleDataSource = m_internalDataSource?.Where(Filter).ToObservableCollection();

            if (m_visibleDataSource != null)
                m_visibleDataSource.CollectionChanged += OnVisibleCollectionChanged;
        }

        void OnInternalCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (m_blockInternalCollectionEvent || m_visibleDataSource == m_internalDataSource)
                return;

            m_blockVisibleCollectionEvent = true;

            if (e.OldItems != null)
                for (int i = 0; i < e.OldItems.Count; i++)
                    m_visibleDataSource.Remove((ViewModelType)e.OldItems[i]);

            if (e.NewItems != null)
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    var item = (ViewModelType)e.NewItems[i];

                    if (Filter(item))
                        m_visibleDataSource.Add(item);
                }

            m_blockVisibleCollectionEvent = false;
        }

        void OnVisibleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (m_blockVisibleCollectionEvent || m_visibleDataSource == m_internalDataSource)
                return;

            m_blockInternalCollectionEvent = true;

            if (e.OldItems != null)
                for (int i = 0; i < e.OldItems.Count; i++)
                    m_internalDataSource.Remove((ViewModelType)e.OldItems[i]);

            if (e.NewItems != null)
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    var item = (ViewModelType)e.NewItems[i];

                    if (Filter(item))
                        m_internalDataSource.Add(item);
                }

            m_blockInternalCollectionEvent = false;
        }

        bool IsSameSelection(ViewModelType[] newSelection)
        {
            if (newSelection == null || newSelection.Length == 0)
                return _selectedItemsIndexes == null;

            if (_selectedItemsIndexes.Length != newSelection.Length)
                return false;

            for (int i = 0; i < _selectedItemsIndexes.Length; i++)
            {
                if (SafeDataSource[_selectedItemsIndexes[i]] != newSelection[i])
                    return false;
            }

            return true;
        }

        bool IsSameSelection(int[] newSelection)
        {
            if (newSelection == null || newSelection.Length == 0)
                return _selectedItemsIndexes == null;

            if (_selectedItemsIndexes == null)
                return false;

            if (_selectedItemsIndexes.Length != newSelection.Length)
                return false;

            for (int i = 0; i < _selectedItemsIndexes.Length; i++)
            {
                if (_selectedItemsIndexes[i] != newSelection[i])
                    return false;
            }

            return true;
        }

        IList<ViewModelType> SafeDataSource => (IList<ViewModelType>)DataSource ?? _emptyDataSource; 

        IEnumerable<ViewModelType> IListFrameOut<ViewModelType>.DataSource => DataSource;

        IEnumerable IListFrame.DataSource => DataSource;
        
        Func<ViewModelType, long> IListFrameIn<ViewModelType>.GetItemIdSelector() => GetItemId;
        
        void IListFrameOut<ViewModelType>.SetItemIdSelector(Func<ViewModelType, long> selector) => GetItemId = selector;

        ViewModelType IListFrameOut<ViewModelType>.GetItemAtIndex(int index) => SafeDataSource[index];

        int IListFrameIn<ViewModelType>.IndexOf(ViewModelType item) => SafeDataSource.IndexOf(item);

        Func<string, string> IListFrameIn<ViewModelType>.GetCategoryStyleSelector() => GetCategoryViewStyle;

        void IListFrameOut<ViewModelType>.SetCategoryStyleSelector(Func<string, string> selector) => GetCategoryViewStyle = selector;

        string IListFrame.GetCategoryStyle(string category) => GetCategoryViewStyle?.Invoke(category) ?? _defaultStyle;

        long IListFrameIn<ViewModelType>.GetItemId(ViewModelType item)
        {
            if (GetItemId == null)
            {
                var hashBase = (object)item ?? string.Empty;
                long hash = (hashBase.GetHashCode() << 16) + hashBase.ToString().GetHashCode();
                return hash;
            }

            return GetItemId(item);
        }

        string IListFrameIn<ViewModelType>.GetItemViewStyle(ViewModelType item)
        {
            return item.GetViewStyle();
        }

        ViewModel IListFrame.GetCategoryModel(string category)
        {
            if (m_categoriesModels != null && m_categoriesModels.TryGetValue(category, out ViewModel model))
                return model;
            
            var newModel = GetCategoryModel?.Invoke(category, GetCategoryViewStyle?.Invoke(category));

            if (newModel == null)
                return null;

            if (m_categoriesModels == null)
                m_categoriesModels = new Dictionary<string, ViewModel>();

            return m_categoriesModels[category] = newModel;
        }
        
        bool IListFrame.ShouldShowElement(ViewModel element) => Filter?.Invoke((ViewModelType)element) ?? true;

        ViewModel IListFrame.GetItemAtIndex(int index) => SafeDataSource[index];

        int IListFrame.IndexOf(ViewModel item) => SafeDataSource.IndexOf((ViewModelType)item);

        long IListFrame.GetItemId(ViewModel item)
        {
            if (GetItemId == null)
            {
                var hashBase = (object)item ?? string.Empty;
                long hash = (hashBase.GetHashCode() << 16) + hashBase.ToString().GetHashCode();
                return hash;
            }

            return GetItemId((ViewModelType)item);
        }

        string IListFrame.GetItemViewStyle(ViewModel item) => item.GetViewStyle() ?? _defaultStyle;

        void IListFrame.ItemClick(ViewModel item)
        {
            ItemClick((ViewModelType)item);
        }
        void IListFrame.ItemLongClick(ViewModel item)
        {
            ItemLongClick((ViewModelType)item);
        }

        string IListFrame.GetItemCategory(ViewModel item)
        {
            return item.Category;
        }
    }
}
