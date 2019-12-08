namespace SharedServices
{
    public class NewUserMessage : Message
    {
        public NewUserMessage()
            : base(MessageTypes.NewUser)
        {
        }

        public long? Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
