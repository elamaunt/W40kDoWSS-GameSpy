namespace Framework
{
    public interface IScrollManager
    {
        float ScrollOffset { get; set; }
        float MaxScrollOffset { get; }
        void ScrollToOffsetAnimated(int offset, float duration);
        void ScrollToItem(ItemViewModel item, ScrollPosition position = ScrollPosition.None, bool animated = true);
        void ScrollToItem(int index, ScrollPosition position = ScrollPosition.None, bool animated = true);
        bool IsItemVisible(ItemViewModel item);
        bool IsScrolledToEnd();
        bool IsScrolledToStart();
        object[] GetVisibleItems();
    }
}
