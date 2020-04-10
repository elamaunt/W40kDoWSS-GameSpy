using System.Collections.Generic;

namespace SharedServices
{
    public class UpdateLobbyMessage : Message
    {
        public UpdateLobbyMessage()
            : base(MessageTypes.UpdateLobby)
        {
        }

        public bool? Joinable { get; set; }
        public int? MaxPlayers { get; set; }
        public string Indicator { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
}
