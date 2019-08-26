using System;

namespace ApiDomain
{
    public class NewsItemDTO
    {
        public long Id { get; set; }
        public string Author { get; set; }
        public string NewsType { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime EditedDate { get; set; }
        public string ImageBase64 { get; set; }
        public string ImagePath { get; set; }

        public NewsLanguageItemDTO Russian { get; set; }
        public NewsLanguageItemDTO English { get; set; }
    }
}
