using Lidgren.Network;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;

namespace SharedServices
{
    public static class NetConnectionExtensionMethods
    {
        private static readonly JsonSerializerSettings Settings;

        static NetConnectionExtensionMethods()
        {
            Settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Culture = CultureInfo.InvariantCulture
            };
        }
        public static void WriteJsonMessage(this NetOutgoingMessage self, UserConnectedMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.UserConnected;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, UserDisconnectedMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.UserDisconnected;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, UserProfileChangedMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.UserProfileChanged;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, UsersMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.Users;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, UserStatsMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.UserStats;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, GameBroadcastMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.GameBroadcast;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, ChatMessageMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.ChatMessage;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, UserStatsChangedMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.UserStatsChanged;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, UserStatusChangedMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.UserStatusChanged;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, RequestUserStatsMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.RequestUserStats;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, LoginMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.Login;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, LogoutMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.Logout;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, GameFinishedMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.GameFinished;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, RequestUsersMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.RequestUsers;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, NewProfileMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.NewProfile;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, RegisterErrorMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.RegisterError;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, LoginInfoMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.LoginInfo;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }
        public static void WriteJsonMessage(this NetOutgoingMessage self, RequestPlayersTopMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.RequestPlayersTop;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WritePlayersTopJsonMessage(this NetOutgoingMessage self, string jsonMessage)
        {
            var type = MessageTypes.PlayersTop;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {jsonMessage}");

            self.Write((byte)type);
            self.Write(jsonMessage);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, PlayersTopMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.PlayersTop;

            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }

        public static void WriteJsonMessage(this NetOutgoingMessage self, RequestLastGamesMessage message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = MessageTypes.RequestLastGames;
            if (Debugger.IsAttached)
                Console.WriteLine($"SEND {type}: {json}");

            self.Write((byte)type);
            self.Write(json);
        }
        

        public static void ReadJsonMessage(this NetIncomingMessage self, IClientMessagesHandler handler)
        {
            var type = (MessageTypes)self.ReadByte();
            var json = self.ReadString();

            if (Debugger.IsAttached)
                Console.WriteLine($"RECV {type}: {json}");

            switch (type)
            {
                case MessageTypes.ChatMessage: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<ChatMessageMessage>(json, Settings)); break;
                case MessageTypes.UserStatusChanged: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<UserStatusChangedMessage>(json, Settings)); break;
                case MessageTypes.GameBroadcast: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<GameBroadcastMessage>(json, Settings)); break;
                case MessageTypes.RequestUserStats: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<RequestUserStatsMessage>(json, Settings)); break;
                case MessageTypes.Login: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<LoginMessage>(json, Settings)); break;
                case MessageTypes.Logout: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<LogoutMessage>(json, Settings)); break;
                case MessageTypes.GameFinished: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<GameFinishedMessage>(json, Settings)); break;
                case MessageTypes.RequestUsers: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<RequestUsersMessage>(json, Settings)); break;
                case MessageTypes.RequestPlayersTop: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<RequestPlayersTopMessage>(json, Settings)); break;
                case MessageTypes.RequestLastGames: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<RequestLastGamesMessage>(json, Settings)); break;
                default:
                    break;
            }
        }

        public static void ReadJsonMessage(this NetIncomingMessage self, IServerMessagesHandler handler)
        {
            var type = (MessageTypes)self.ReadByte();
            var json = self.ReadString();

            if (Debugger.IsAttached)
                Console.WriteLine($"RECV {type}: {json}");

            switch (type)
            {
                case MessageTypes.LoginInfo: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<LoginInfoMessage>(json, Settings)); break;
                case MessageTypes.RegisterError: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<RegisterErrorMessage>(json, Settings)); break;
                case MessageTypes.NewProfile: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<NewProfileMessage>(json, Settings)); break;
                case MessageTypes.UserDisconnected: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<UserDisconnectedMessage>(json, Settings)); break;
                case MessageTypes.UserConnected: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<UserConnectedMessage>(json, Settings)); break;
                case MessageTypes.ChatMessage: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<ChatMessageMessage>(json, Settings)); break;
                case MessageTypes.Users: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<UsersMessage>(json, Settings)); break;
                case MessageTypes.UserProfileChanged: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<UserProfileChangedMessage>(json, Settings)); break;
                case MessageTypes.UserStatusChanged: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<UserStatusChangedMessage>(json, Settings)); break;
                case MessageTypes.GameBroadcast: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<GameBroadcastMessage>(json, Settings)); break;
                case MessageTypes.UserStatsChanged: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<UserStatsChangedMessage>(json, Settings)); break;
                case MessageTypes.UserStats: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<UserStatsMessage>(json, Settings)); break;
                case MessageTypes.PlayersTop: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<PlayersTopMessage>(json, Settings)); break;
                case MessageTypes.LastGames: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<LastGamesMessage>(json, Settings)); break;
                default:
                    break;
            }
        }
    }
}
