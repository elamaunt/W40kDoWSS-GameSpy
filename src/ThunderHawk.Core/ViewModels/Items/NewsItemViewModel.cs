using Framework;
using System;

namespace ThunderHawk.Core
{
    public class NewsItemViewModel : ItemViewModel
    {
        public TextFrame Title { get; } = new TextFrame();
        public TextFrame Annotation { get; } = new TextFrame();
        public ValueFrame<DateTime> Date { get; } = new ValueFrame<DateTime>() { ValueToTextConverter = ConvertValueToText };
        public UriFrame Image { get; } = new UriFrame();
        public ActionFrame Navigate { get; } = new ActionFrame();

        public NewsItemDTO NewsItem { get; }

        public bool Big { get; set; }

        public NewsItemViewModel(NewsItemDTO dto)
        {
            NewsItem = dto;

            Title.Text = dto.RussianTitle;
            Annotation.Text = dto.RussianAnnotation;
            Date.Value = dto.NewsTime;
            Image.Text = dto.ImagePath;
        }

        public override string GetViewStyle()
        {
            if (Big)
                return "Big" + base.GetViewStyle();

            return base.GetViewStyle();
        }
        
        private static string ConvertValueToText(DateTime date)
        {
            return date.ToLongDateString();
        }
    }
}
