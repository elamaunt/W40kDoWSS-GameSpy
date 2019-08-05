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

        public string image; //Image representation by its path.

        public NewsType newsType;

        public long newsTime;

        public uint newsId; // Unique identifier for this news

        public long newsEditTime;


        public News(string rusHeader, string rusPreviewText, string rusText,
            string engHeader, string engPreviewText, string engText, string img,
            NewsType type, long time, long editTime, uint id)
        {
            russianHeader = rusHeader;
            englishHeader = engHeader;

            russianPreviewText = rusPreviewText;
            englishPreviewText = engPreviewText;

            russianText = rusText;
            englishText = engText;

            image = img;

            newsType = type;

            newsTime = time;
            newsEditTime = editTime;

            newsId = id;
        }
    }

    public enum NewsType
    {
        Tournament,
        Update,
        Event
    }
}
