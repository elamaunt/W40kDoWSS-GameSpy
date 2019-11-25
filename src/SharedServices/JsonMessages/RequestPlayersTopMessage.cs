namespace SharedServices
{
    public class RequestPlayersTopMessage : Message
    {
        public RequestPlayersTopMessage()
            : base(MessageTypes.RequestPlayersTop)
        {
        }

        public int Offset { get; set; }
        public int Count { get; set; }
    }
}
