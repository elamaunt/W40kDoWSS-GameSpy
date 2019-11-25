namespace SharedServices
{
    public class LoginInfoMessage : Message
    {
        public LoginInfoMessage()
            : base(MessageTypes.LoginInfo)
        {
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public string PassEnc { get; set; }
        public long ProfileId { get; set; }
    }
}
