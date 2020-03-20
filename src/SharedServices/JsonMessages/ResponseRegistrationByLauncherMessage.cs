namespace SharedServices
{
    public class ResponseRegistrationByLauncherMessage : Message
    {
        public ResponseRegistrationByLauncherMessage()
            : base(MessageTypes.ResponseRegistrationByLauncher)
        {
        }

        public bool RegistrationSuccess { get; set; }
    }
}
