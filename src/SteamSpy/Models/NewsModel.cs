using Common;
using System;

namespace SteamSpy.Models
{
    public class NewsModel
    {
        // Properties for VIEW BINDING
        public string ShortText { get; private set; }
        public string Header { get; private set; }

        public string NewsTime
        {
            get
            {
                var dt = new DateTime(rawNews.newsTime).ToLocalTime();
                return $"{dt.ToString(@"hh\:mm")}";
            }
        }

        public void UpdateText(bool isRus)
        {
            if (isRus)
            {
                ShortText = rawNews.russianPreviewText;
                Header = rawNews.russianHeader;
            }
            else
            {
                ShortText = rawNews.englishPreviewText;
                Header = rawNews.englishHeader;
            }
        }


        private readonly News rawNews;

        public NewsModel(News news)
        {
            rawNews = news;
            UpdateText(true);
        }
    }
}
