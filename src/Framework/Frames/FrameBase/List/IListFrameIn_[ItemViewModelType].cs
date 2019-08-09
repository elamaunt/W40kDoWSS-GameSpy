using System;

namespace Framework
{
    public interface IListFrameIn<in ItemViewModelType> : IListFrame
        where ItemViewModelType : ViewModel
    {
        int IndexOf(ItemViewModelType item);
        long GetItemId(ItemViewModelType item);
        string GetItemViewStyle(ItemViewModelType item);
        
        Func<string, string> GetCategoryStyleSelector();
        Func<ItemViewModelType, long> GetItemIdSelector();
    }
}
