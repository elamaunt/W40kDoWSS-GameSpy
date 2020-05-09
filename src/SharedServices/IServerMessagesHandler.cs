using Lidgren.Network;

namespace SharedServices
{
    public interface IServerMessagesHandler
    {
        void HandleMessage(NetConnection connection, UserDisconnectedMessage message);
        void HandleMessage(NetConnection connection, UserConnectedMessage message);
        void HandleMessage(NetConnection connection, ChatMessageMessage message);
        void HandleMessage(NetConnection connection, UsersMessage message);
        void HandleMessage(NetConnection connection, UserProfileChangedMessage message);
        void HandleMessage(NetConnection connection, UserStatusChangedMessage message);
        void HandleMessage(NetConnection connection, GameBroadcastMessage message);
        void HandleMessage(NetConnection connection, UserStatsChangedMessage message);
        void HandleMessage(NetConnection connection, UserStatsMessage message);
        void HandleMessage(NetConnection connection, NewProfileMessage message);
        void HandleMessage(NetConnection connection, RegisterErrorMessage message);
        void HandleMessage(NetConnection connection, LoginInfoMessage message);
        void HandleMessage(NetConnection senderConnection, PlayersTopMessage message);
        void HandleMessage(NetConnection senderConnection, LastGamesMessage message);
        void HandleMessage(NetConnection senderConnection, AllUserNicksMessage message);
        void HandleMessage(NetConnection senderConnection, ResponseCanAuthorizeMessage message);
        void HandleMessage(NetConnection senderConnection, ResponseRegistrationByLauncherMessage message);
        void HandleMessage(NetConnection senderConnection, NameCheckMessage message);
        void HandleMessage(NetConnection senderConnection, LoginErrorMessage message);
        void HandleMessage(NetConnection senderConnection, SetKeyValueMessage message);
        void HandleMessage(NetConnection senderConnection, NewUserMessage newUserMessage);
        void HandleMessage(NetConnection connection, GameFinishedMessage message);
        void HandleMessage(NetConnection senderConnection, EnteredInLobbyMessage enteredInLobbyMessage);
        void HandleMessage(NetConnection senderConnection, LobbyChatLineMessage lobbyChatLineMessage);
        void HandleMessage(NetConnection senderConnection, LobbyKeyValueMessage lobbyKeyValueMessage);
        void HandleMessage(NetConnection senderConnection, LobbiesMessage lobbiesMessage);
        void HandleMessage(NetConnection senderConnection, LobbyCreatedMessage lobbyCreatedMessage);
        void HandleMessage(NetConnection senderConnection, LobbyLeftMessage lobbyLeftMessage);
    }
}