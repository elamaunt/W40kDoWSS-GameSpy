namespace SharedServices
{
    public class CreateLobbyMessage : Message
    {
        public CreateLobbyMessage()
            : base(MessageTypes.CreateLobby)
        {
        }

        public string Name { get; set; }
        public string ShortUser { get; set; }
        public string Flags { get; set; }
        public string Indicator { get; set; }
    }
}
