namespace SharedServices
{
    public class EnterLobbyMessage : Message
    {
        public EnterLobbyMessage() 
            : base(MessageTypes.EnterLobby)
        {
        }

        public ulong HostId { get; set; }
        public string ShortUser { get; set; }
        public string Name { get; set; }
        public string ProfileId { get; set; }
        public string LocalRoomHash { get; set; }
    }
}
