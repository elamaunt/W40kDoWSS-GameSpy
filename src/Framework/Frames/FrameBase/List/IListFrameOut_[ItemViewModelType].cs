using System;
using System.Collections.Generic;

namespace Framework
{
    public interface IListFrameOut<out ItemViewModelType> : IListFrame
        where ItemViewModelType : ViewModel
    {
        new IEnumerable<ItemViewModelType> DataSource { get; }
        new ItemViewModelType GetItemAtIndex(int index);
        
        void SetCategoryStyleSelector(Func<string, string> selector);
        void SetItemIdSelector(Func<ItemViewModelType, long> selector);

    }
}
