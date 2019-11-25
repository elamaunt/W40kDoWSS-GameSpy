namespace SharedServices
{
    public class NewProfileMessage : Message
    {
        public NewProfileMessage()
            : base(MessageTypes.NewProfile)
        {
        }

        public ulong SteamId { get; set; }
        public string Name { get; set; }
        public long Id { get; set; }
    }
}
