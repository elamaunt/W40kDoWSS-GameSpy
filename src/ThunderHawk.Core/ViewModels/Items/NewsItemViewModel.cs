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
        
        public NewsItemDTO NewsItem { get; }

        public NewsItemViewModel(NewsItemDTO dto)
        {
            NewsItem = dto;

            Title.Text = dto.RussianTitle;
            Annotation.Text = dto.RussianAnnotation;
            Date.Value = dto.NewsTime;
            Image.Text = dto.ImagePath;
        }

        private static string ConvertValueToText(DateTime date)
        {
            return date.ToLongDateString();
        }
    }
}
