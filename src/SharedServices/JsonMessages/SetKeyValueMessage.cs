namespace SharedServices
{
    public class SetKeyValueMessage : Message
    {
        public SetKeyValueMessage() 
            : base(MessageTypes.SetKeyValue)
        {
        }
        public ulong SteamId { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
