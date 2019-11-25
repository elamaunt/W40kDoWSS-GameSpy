namespace SharedServices
{
    public class LoginMessage : Message
    {
        public LoginMessage()
            : base(MessageTypes.Login)
        {
        }

        public string Name { get; set; }

        public bool NeedsInfo { get; set; }
    }
}
