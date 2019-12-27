namespace SharedServices
{
    public class ChatMessageMessage : Message
    {
        public ChatMessageMessage()
            : base(MessageTypes.ChatMessage)
        {
        }

        public ulong SteamId { get; set; }
        public string Text { get; set; }
        public long LongDate { get; set; }
        public bool FromGame { get; set; }

        public string UserName { get; set; }
    }
}
