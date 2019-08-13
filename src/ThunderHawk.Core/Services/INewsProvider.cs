namespace ThunderHawk.Core
{
    public interface INewsProvider
    {
        // If no news edited, deleted or added it will return null. 
        // Otherwise - it will return full news list ordered by time.
        NewsItemDTO[] GetNews();
    }
}
