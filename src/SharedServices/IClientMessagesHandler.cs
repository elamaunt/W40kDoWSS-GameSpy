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
        void HandleMessage(NetConnection senderConnection, RequestPlayersTopMessage requestPlayersTopMessage);
        void HandleMessage(NetConnection senderConnection, RequestLastGamesMessage requestLastGamesMessage);
        void HandleMessage(NetConnection senderConnection, RequestAllUserNicksMessage requestAllUserNicksMessage);
        void HandleMessage(NetConnection senderConnection, RequestCanAuthorizeMessage requestCanAuthorizeMessage);
        void HandleMessage(NetConnection senderConnection, RequestRegistrationByLauncher requestRegistrationByLauncher);
        void HandleMessage(NetConnection senderConnection, RequestNameCheckMessage requestNameCheckMessage);
        void HandleMessage(NetConnection senderConnection, SetKeyValueMessage setKeyValueMessage);
        void HandleMessage(NetConnection senderConnection, RequestNewUserMessage requestNewUserMessage);
    }
}