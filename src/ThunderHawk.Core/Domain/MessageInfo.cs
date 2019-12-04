using System;

namespace ThunderHawk.Core
{
    public class MessageInfo
    {
        public UserInfo Author { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public bool FromGame { get; set; }
        public bool IsPrivate { get; set; }
    }
}
