namespace SharedServices
{
    public enum MessageTypes : byte
    {
        UserDisconnected,
        UserConnected,
        ChatMessage,
        Users,
        UserNameChanged,
        UserStatusChanged,
        GameBroadcast,
        UserStatsChanged,
        UserStats,
        RequestUserStats,
        Login,
        Logout,
        GameFinished,
        RequestUsers
    }
}
