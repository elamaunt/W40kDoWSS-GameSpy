namespace SharedServices
{
    public class RequestUsersMessage : Message
    {
        public RequestUsersMessage()
            : base(MessageTypes.RequestUsers)
        {
        }

    }
}
