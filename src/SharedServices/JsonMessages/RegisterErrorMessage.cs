namespace SharedServices
{
    public class RegisterErrorMessage : Message
    {
        public RegisterErrorMessage()
            : base(MessageTypes.RegisterError)
        {
        }

        public string Name { get; set; }
        public string Message { get; set; }
    }
}
