namespace SharedServices
{
    public class RequestAllUserNicksMessage : Message
    {
        public RequestAllUserNicksMessage()
            : base(MessageTypes.RequestAllUserNicks)
        {
        }

        public string Email { get; set; }
    }
}
