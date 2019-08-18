using System;

namespace ThunderHawk.Core
{
    public class NewsItemDTO
    {
        public string Author { get; set; }
        public string RussianTitle { get; set; }
        public string RussianAnnotation { get; set; }
        public string RussianText { get; set; }
        
        public string EnglishTitle { get; set; }
        public string EnglishAnnotation { get; set; }
        public string EnglishText { get; set; }

        public string ImagePath { get; set; }
        public NewsType NewsType { get; set; }
        public DateTime NewsTime { get; set; }
        public uint NewsId { get; set; }
        public DateTime NewsEditTime { get; set; }
    }
}
