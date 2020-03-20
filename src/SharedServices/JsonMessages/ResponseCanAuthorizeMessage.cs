namespace SharedServices
{
    public class ResponseCanAuthorizeMessage : Message
    {
        public ResponseCanAuthorizeMessage()
            : base(MessageTypes.ResponseCanAuthorize)
        {
        }

        public bool CanAuthorize { get; set; }
    }
}
