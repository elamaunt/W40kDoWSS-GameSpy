namespace SharedServices
{
    public class UsersMessage : Message
    {
        public UsersMessage()
            : base(MessageTypes.Users)
        {
        }

        public UserPart[] Users { get; set; }
    }
}
