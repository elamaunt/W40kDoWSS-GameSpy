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

        public static void WriteJsonMessage(this NetOutgoingMessage self, Message message)
        {
            var json = JsonConvert.SerializeObject(message, Settings);
            var type = message.Type;

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
                case MessageTypes.RequestAllUserNicks: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<RequestAllUserNicksMessage>(json, Settings)); break;
                case MessageTypes.RequestCanAuthorize: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<RequestCanAuthorizeMessage>(json, Settings)); break;
                case MessageTypes.RequestRegistrationByLauncher: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<RequestRegistrationByLauncher>(json, Settings)); break;
                case MessageTypes.RequestNameCheck: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<RequestNameCheckMessage>(json, Settings)); break;
                case MessageTypes.SetKeyValue: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<SetKeyValueMessage>(json, Settings)); break;
                case MessageTypes.RequestNewUser: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<RequestNewUserMessage>(json, Settings)); break;
                case MessageTypes.LeaveLobby: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<LeaveLobbyMessage>(json, Settings)); break;
                case MessageTypes.EnterLobby: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<EnterLobbyMessage>(json, Settings)); break;
                case MessageTypes.CreateLobby: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<CreateLobbyMessage>(json, Settings)); break;
                case MessageTypes.UpdateLobby: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<UpdateLobbyMessage>(json, Settings)); break;
                case MessageTypes.LobbyChatLine: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<LobbyChatLineMessage>(json, Settings)); break;
                case MessageTypes.LobbyKeyValue: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<LobbyKeyValueMessage>(json, Settings)); break;
                case MessageTypes.RequestLobbies: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<RequestLobbiesMessage>(json, Settings)); break;
                case MessageTypes.LobbyGameStarted: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<LobbyGameStartedMessage>(json, Settings)); break;
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
                case MessageTypes.AllUserNicks: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<AllUserNicksMessage>(json, Settings)); break;
                case MessageTypes.ResponseCanAuthorize: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<ResponseCanAuthorizeMessage>(json, Settings)); break;
                case MessageTypes.ResponseRegistrationByLauncher: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<ResponseRegistrationByLauncherMessage>(json, Settings)); break;
                case MessageTypes.NameCheck: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<NameCheckMessage>(json, Settings)); break;
                case MessageTypes.LoginError: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<LoginErrorMessage>(json, Settings)); break;
                case MessageTypes.SetKeyValue: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<SetKeyValueMessage>(json, Settings)); break;
                case MessageTypes.NewUser: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<NewUserMessage>(json, Settings)); break;
                case MessageTypes.GameFinished: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<GameFinishedMessage>(json, Settings)); break;
                case MessageTypes.EnteredInLobby: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<EnteredInLobbyMessage>(json, Settings)); break;
                case MessageTypes.LobbyChatLine: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<LobbyChatLineMessage>(json, Settings)); break;
                case MessageTypes.LobbyKeyValue: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<LobbyKeyValueMessage>(json, Settings)); break;
                case MessageTypes.Lobbies: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<LobbiesMessage>(json, Settings)); break;
                case MessageTypes.LobbyCreated: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<LobbyCreatedMessage>(json, Settings)); break;
                case MessageTypes.LobbyLeft: handler.HandleMessage(self.SenderConnection, JsonConvert.DeserializeObject<LobbyLeftMessage>(json, Settings)); break;
                default:
                    break;
            }
        }
    }
}
