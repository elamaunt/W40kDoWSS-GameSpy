using ApiDomain;
using Framework;
using System;

namespace ThunderHawk.Core
{
    public class NewsViewerPageViewModel : EmbeddedPageViewModel
    {
        public ButtonFrame Text { get; } = new ButtonFrame() { ActionWithParameter = OnUriClicked };

        public TextFrame Author { get; } = new TextFrame();
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
                Author.Text = NewsItem.Author?.ToUpperInvariant();

                Date.Value = NewsItem.CreatedDate;
            }

            base.OnPassData(bundle);
        }
        private static string ConvertValueToText(DateTime date)
        {
            return date.ToLongDateString();
        }

        private static void OnUriClicked(object obj)
        {
            var uri = obj as Uri;

            if (uri == null)
                return;

            CoreContext.SystemService.OpenLink(uri);
        }
    }
}
