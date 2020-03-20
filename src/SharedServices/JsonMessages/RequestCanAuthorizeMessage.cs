namespace SharedServices
{
    public class RequestCanAuthorizeMessage : Message
    {
        public RequestCanAuthorizeMessage()
            : base(MessageTypes.RequestCanAuthorize)
        {
        }

        public string Login { get; set; }
        public string Password { get; set; }
    }
}
