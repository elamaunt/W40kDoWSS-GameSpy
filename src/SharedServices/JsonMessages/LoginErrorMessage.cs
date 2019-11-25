namespace SharedServices
{
    public class LoginErrorMessage : Message
    {
        public LoginErrorMessage()
            : base(MessageTypes.LoginError)
        {
        }

        public string Name { get; set; }
    }
}
