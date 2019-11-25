namespace SharedServices
{
    public class LogoutMessage : Message
    {
        public LogoutMessage()
            : base(MessageTypes.Logout)
        {
        }
    }
}
