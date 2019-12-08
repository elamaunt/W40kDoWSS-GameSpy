using System.Collections.Generic;

namespace SharedServices
{
    public class RequestNewUserMessage : Message
    {
        public RequestNewUserMessage()
            : base(MessageTypes.RequestNewUser)
        {
        }

        public Dictionary<string, string> KeyValues { get; set; }
    }
}
