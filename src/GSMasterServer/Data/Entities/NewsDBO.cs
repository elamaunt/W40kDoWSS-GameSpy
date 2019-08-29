using System;

namespace GSMasterServer.Data
{
    public class NewsDBO
    {
        public long Id { get; set; }
        public string Author { get; set; }
        public string NewsType { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime EditedDate { get; set; }
        public string ImageBase64 { get; set; }
        public string ImagePath { get; set; }
        
        public NewsData Russian { get; set; }
        public NewsData English { get; set; }
    }
}
