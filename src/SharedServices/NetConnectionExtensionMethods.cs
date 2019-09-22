using Lidgren.Network;
using Newtonsoft.Json;

namespace SharedServices
{
    public static class NetConnectionExtensionMethods
    {
        public static void WriteJsonMessage(this NetOutgoingMessage self, UserConnectedMessage message)
        {
            self.Write((byte)MessageTypes.UserConnected);
            self.Write(JsonConvert.SerializeObject(message));
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, UserDisconnectedMessage message)
        {
            self.Write((byte)MessageTypes.UserDisconnected);
            self.Write(JsonConvert.SerializeObject(message));
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, UserNameChangedMessage message)
        {
            self.Write((byte)MessageTypes.UserNameChanged);
            self.Write(JsonConvert.SerializeObject(message));
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, UsersMessage message)
        {
            self.Write((byte)MessageTypes.Users);
            self.Write(JsonConvert.SerializeObject(message));
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, UserStatsMessage message)
        {
            self.Write((byte)MessageTypes.UserStats);
            self.Write(JsonConvert.SerializeObject(message));
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, GameBroadcastMessage message)
        {
            self.Write((byte)MessageTypes.GameBroadcast);
            self.Write(JsonConvert.SerializeObject(message));
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, ChatMessageMessage message)
        {
            self.Write((byte)MessageTypes.ChatMessage);
            self.Write(JsonConvert.SerializeObject(message));
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, UserStatsChangedMessage message)
        {
            self.Write((byte)MessageTypes.UserStatsChanged);
            self.Write(JsonConvert.SerializeObject(message));
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, UserStatusChangedMessage message)
        {
            self.Write((byte)MessageTypes.UserStatusChanged);
            self.Write(JsonConvert.SerializeObject(message));
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, RequestUserStatsMessage message)
        {
            self.Write((byte)MessageTypes.RequestUserStats);
            self.Write(JsonConvert.SerializeObject(message));
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, LoginMessage message)
        {
            self.Write((byte)MessageTypes.Login);
            self.Write(JsonConvert.SerializeObject(message));
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, LogoutMessage message)
        {
            self.Write((byte)MessageTypes.Logout);
            self.Write(JsonConvert.SerializeObject(message));
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, GameFinishedMessage message)
        {
            self.Write((byte)MessageTypes.GameFinished);
            self.Write(JsonConvert.SerializeObject(message));
        }

        public static void ReadJsonMessage(this NetOutgoingMessage self, IMessagesHandler handler)
        {
            switch ((MessageTypes)self.ReadByte())
            {
                case MessageTypes.UserDisconnected: handler.HandleMessage(JsonConvert.DeserializeObject<UserDisconnectedMessage>(self.ReadString())); break;
                case MessageTypes.UserConnected: handler.HandleMessage(JsonConvert.DeserializeObject<UserConnectedMessage>(self.ReadString())); break;
                case MessageTypes.ChatMessage: handler.HandleMessage(JsonConvert.DeserializeObject<ChatMessageMessage>(self.ReadString())); break;
                case MessageTypes.Users: handler.HandleMessage(JsonConvert.DeserializeObject<UsersMessage>(self.ReadString())); break;
                case MessageTypes.UserNameChanged: handler.HandleMessage(JsonConvert.DeserializeObject<UserNameChangedMessage>(self.ReadString())); break;
                case MessageTypes.UserStatusChanged: handler.HandleMessage(JsonConvert.DeserializeObject<UserStatusChangedMessage>(self.ReadString())); break;
                case MessageTypes.GameBroadcast: handler.HandleMessage(JsonConvert.DeserializeObject<GameBroadcastMessage>(self.ReadString())); break;
                case MessageTypes.UserStatsChanged: handler.HandleMessage(JsonConvert.DeserializeObject<UserStatsChangedMessage>(self.ReadString())); break;
                case MessageTypes.UserStats: handler.HandleMessage(JsonConvert.DeserializeObject<UserStatsMessage>(self.ReadString())); break;
                case MessageTypes.RequestUserStats: handler.HandleMessage(JsonConvert.DeserializeObject<RequestUserStatsMessage>(self.ReadString())); break;
                case MessageTypes.Login: handler.HandleMessage(JsonConvert.DeserializeObject<LoginMessage>(self.ReadString())); break;
                case MessageTypes.Logout: handler.HandleMessage(JsonConvert.DeserializeObject<LogoutMessage>(self.ReadString())); break;
                case MessageTypes.GameFinished: handler.HandleMessage(JsonConvert.DeserializeObject<GameFinishedMessage>(self.ReadString())); break;
                default:
                    break;
            }
        }
    }
}
