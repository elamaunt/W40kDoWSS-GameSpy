using Framework;
using System;

namespace ThunderHawk.Core
{
    public class NewsViewerPageViewModel : EmbeddedPageViewModel
    {
        public TextFrame Text { get; } = new TextFrame();
        public TextFrame Annotation { get; } = new TextFrame();
        public ValueFrame<DateTime> Date { get; } = new ValueFrame<DateTime>() { ValueToTextConverter = ConvertValueToText };
        public UriFrame Image { get; } = new UriFrame();

        public NewsItemDTO NewsItem { get; private set; }

        protected override void OnPassData(IDataBundle bundle)
        {
            NewsItem = bundle.GetString(nameof(NewsItem)).OfJson<NewsItemDTO>();

            if (NewsItem != null)
            {
                Image.Text = NewsItem.ImagePath;

                TitleButton.Text = NewsItem.RussianTitle;
                Annotation.Text = NewsItem.RussianAnnotation;
                Text.Text = NewsItem.RussianText;

                Date.Value = NewsItem.NewsTime;
            }

            base.OnPassData(bundle);
        }
        private static string ConvertValueToText(DateTime date)
        {
            return date.ToLongDateString();
        }
    }
}
