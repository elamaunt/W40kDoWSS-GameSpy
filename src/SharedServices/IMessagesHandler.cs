namespace SharedServices
{
    public interface IMessagesHandler
    {
        void HandleMessage(UserDisconnectedMessage message);
        void HandleMessage(UserConnectedMessage message);
        void HandleMessage(ChatMessageMessage message);
        void HandleMessage(UsersMessage message);
        void HandleMessage(UserNameChangedMessage message);
        void HandleMessage(UserStatusChangedMessage message);
        void HandleMessage(GameBroadcastMessage message);
        void HandleMessage(UserStatsChangedMessage message);
        void HandleMessage(UserStatsMessage message);
        void HandleMessage(RequestUserStatsMessage message);
        void HandleMessage(LoginMessage message);
        void HandleMessage(LogoutMessage message);
        void HandleMessage(GameFinishedMessage message);
    }
}