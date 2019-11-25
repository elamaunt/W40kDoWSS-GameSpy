using Newtonsoft.Json;

namespace SharedServices
{
    public abstract class Message
    {
        [JsonIgnore]
        public readonly MessageTypes Type;

        public Message(MessageTypes type)
        {
            Type = type;
        }
    }
}
