using Lidgren.Network;

namespace SharedServices
{
    public interface IClientMessagesHandler
    {
        void HandleMessage(NetConnection connection, ChatMessageMessage message);
        void HandleMessage(NetConnection connection, UserStatusChangedMessage message);
        void HandleMessage(NetConnection connection, GameBroadcastMessage message);
        void HandleMessage(NetConnection connection, RequestUserStatsMessage message);
        void HandleMessage(NetConnection connection, LoginMessage message);
        void HandleMessage(NetConnection connection, LogoutMessage message);
        void HandleMessage(NetConnection connection, GameFinishedMessage message);
        void HandleMessage(NetConnection connection, RequestUsersMessage message);
        void HandleMessage(NetConnection senderConnection, RequestPlayersTopMessage message);
        void HandleMessage(NetConnection senderConnection, RequestLastGamesMessage message);
        void HandleMessage(NetConnection senderConnection, RequestAllUserNicksMessage message);
        void HandleMessage(NetConnection senderConnection, RequestCanAuthorizeMessage message);
        void HandleMessage(NetConnection senderConnection, RequestRegistrationByLauncher message);
        void HandleMessage(NetConnection senderConnection, RequestNameCheckMessage message);
        void HandleMessage(NetConnection senderConnection, SetKeyValueMessage message);
        void HandleMessage(NetConnection senderConnection, RequestNewUserMessage message);
        void HandleMessage(NetConnection senderConnection, LeaveLobbyMessage message);
        void HandleMessage(NetConnection senderConnection, EnterLobbyMessage message);
        void HandleMessage(NetConnection senderConnection, CreateLobbyMessage message);
        void HandleMessage(NetConnection senderConnection, UpdateLobbyMessage message);
        void HandleMessage(NetConnection senderConnection, LobbyChatLineMessage message);
        void HandleMessage(NetConnection senderConnection, LobbyKeyValueMessage message);
        void HandleMessage(NetConnection senderConnection, RequestLobbiesMessage message);
        void HandleMessage(NetConnection senderConnection, LobbyGameStartedMessage message);
    }
}