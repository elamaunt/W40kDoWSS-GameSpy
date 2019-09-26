using Lidgren.Network;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace SharedServices
{
    public static class NetConnectionExtensionMethods
    {
        public static void WriteJsonMessage(this NetOutgoingMessage self, UserConnectedMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            var type = MessageTypes.UserConnected;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, UserDisconnectedMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            var type = MessageTypes.UserDisconnected;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, UserNameChangedMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            var type = MessageTypes.UserNameChanged;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, UsersMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            var type = MessageTypes.Users;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, UserStatsMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            var type = MessageTypes.UserStats;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, GameBroadcastMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            var type = MessageTypes.GameBroadcast;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, ChatMessageMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            var type = MessageTypes.ChatMessage;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, UserStatsChangedMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            var type = MessageTypes.UserStatsChanged;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, UserStatusChangedMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            var type = MessageTypes.UserStatusChanged;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, RequestUserStatsMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            var type = MessageTypes.RequestUserStats;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, LoginMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            var type = MessageTypes.Login;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, LogoutMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            var type = MessageTypes.Logout;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, GameFinishedMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            var type = MessageTypes.GameFinished;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, RequestUsersMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            var type = MessageTypes.RequestUsers;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void ReadJsonMessage(this NetIncomingMessage self, IMessagesHandler handler)
        {
            var type = (MessageTypes)self.ReadByte();
            var json = self.ReadString();

            if (Debugger.IsAttached)
                Console.WriteLine($"RECV {type}: {json}");

            switch (type)
            {
                case MessageTypes.UserDisconnected: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<UserDisconnectedMessage>(json)); break;
                case MessageTypes.UserConnected: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<UserConnectedMessage>(json)); break;
                case MessageTypes.ChatMessage: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<ChatMessageMessage>(json)); break;
                case MessageTypes.Users: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<UsersMessage>(json)); break;
                case MessageTypes.UserNameChanged: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<UserNameChangedMessage>(json)); break;
                case MessageTypes.UserStatusChanged: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<UserStatusChangedMessage>(json)); break;
                case MessageTypes.GameBroadcast: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<GameBroadcastMessage>(json)); break;
                case MessageTypes.UserStatsChanged: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<UserStatsChangedMessage>(json)); break;
                case MessageTypes.UserStats: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<UserStatsMessage>(json)); break;
                case MessageTypes.RequestUserStats: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<RequestUserStatsMessage>(json)); break;
                case MessageTypes.Login: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<LoginMessage>(json)); break;
                case MessageTypes.Logout: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<LogoutMessage>(json)); break;
                case MessageTypes.GameFinished: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<GameFinishedMessage>(json)); break;
                case MessageTypes.RequestUsers: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<RequestUsersMessage>(json)); break;
                default:
                    break;
            }
        }
    }
}
