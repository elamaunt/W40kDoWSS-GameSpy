using Lidgren.Network;

namespace SharedServices
{
    public interface IMessagesHandler
    {
        void HandleMessage(NetConnection connection, UserDisconnectedMessage message);
        void HandleMessage(NetConnection connection, UserConnectedMessage message);
        void HandleMessage(NetConnection connection, ChatMessageMessage message);
        void HandleMessage(NetConnection connection, UsersMessage message);
        void HandleMessage(NetConnection connection, UserNameChangedMessage message);
        void HandleMessage(NetConnection connection, UserStatusChangedMessage message);
        void HandleMessage(NetConnection connection, GameBroadcastMessage message);
        void HandleMessage(NetConnection connection, UserStatsChangedMessage message);
        void HandleMessage(NetConnection connection, UserStatsMessage message);
        void HandleMessage(NetConnection connection, RequestUserStatsMessage message);
        void HandleMessage(NetConnection connection, LoginMessage message);
        void HandleMessage(NetConnection connection, LogoutMessage message);
        void HandleMessage(NetConnection connection, GameFinishedMessage message);
        void HandleMessage(NetConnection connection, RequestUsersMessage message);
        
    }
}