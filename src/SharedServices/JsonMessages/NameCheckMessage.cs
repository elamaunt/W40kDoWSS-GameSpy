namespace SharedServices
{
    public class NameCheckMessage : Message
    {
        public NameCheckMessage()
            : base(MessageTypes.NameCheck)
        {
        }

        public string Name { get; set; }
        public long? ProfileId { get; set; }
    }
}
