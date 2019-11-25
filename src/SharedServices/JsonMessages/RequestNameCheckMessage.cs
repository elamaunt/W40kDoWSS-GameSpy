namespace SharedServices
{
    public class RequestNameCheckMessage : Message
    {
        public RequestNameCheckMessage()
            : base(MessageTypes.RequestNameCheck)
        {
        }

        public string Name { get; set; }
    }
}
