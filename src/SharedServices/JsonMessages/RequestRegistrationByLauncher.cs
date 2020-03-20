namespace SharedServices
{
    public class RequestRegistrationByLauncher : Message
    {
        public RequestRegistrationByLauncher()
            : base(MessageTypes.RequestRegistrationByLauncher)
        {
        }

        public string Login { get; set; }
        public string Password { get; set; }
    }
}
