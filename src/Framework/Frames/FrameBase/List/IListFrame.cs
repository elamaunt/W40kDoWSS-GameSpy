using System.Collections;

namespace Framework
{
    public interface IListFrame : IControlFrame
    {
        IEnumerable DataSource { get; }
        bool ShouldShowElement(ViewModel element);
        int ItemsCount { get; }
        ViewModel GetItemAtIndex(int index);
        int IndexOf(ViewModel item);
        long GetItemId(ViewModel item);
        string GetItemViewStyle(ViewModel item);
        bool IsFiltered { get; }
        void Refilter();

        int[] SelectedItemsIndexes { get; set; }
        void ItemLongClick(ViewModel item);
        void ItemClick(ViewModel item);
        string GetItemCategory(ViewModel item);
        string GetCategoryStyle(string category);
        ViewModel GetCategoryModel(string category);
    }
}
