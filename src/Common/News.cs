namespace Common
{
    public class News
    {
        public string russianHeader;
        public string englishHeader;

        public string russianPreviewText;
        public string englishPreviewText;

        public string russianText;
        public string englishText;

        public NewsType newsType;

        public long newsTime;

        public News(string rusHeader, string rusPreviewText, string rusText,
            string engHeader, string engPreviewText, string engText,
            NewsType type, long time)
        {
            russianHeader = rusHeader;
            englishHeader = engHeader;

            russianPreviewText = rusPreviewText;
            englishPreviewText = engPreviewText;

            russianText = rusText;
            englishText = engText;

            newsType = type;
            newsTime = time;
        }
    }

    public enum NewsType
    {
        Tournament,
        Update,
        Event
    }
}
