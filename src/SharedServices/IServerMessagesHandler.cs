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
        void HandleMessage(NetConnection senderConnection, PlayersTopMessage playersTopMessage);
        void HandleMessage(NetConnection senderConnection, LastGamesMessage lastGamesMessage);
        //void HandleMessage(NetConnection connection, GameFinishedMessage message);
    }
}