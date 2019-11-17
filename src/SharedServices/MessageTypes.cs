namespace SharedServices
{
    public enum MessageTypes : byte
    {
        UserDisconnected,
        UserConnected,
        ChatMessage,
        Users,
        UserProfileChanged,
        UserStatusChanged,
        GameBroadcast,
        UserStatsChanged,
        UserStats,
        RequestUserStats,
        Login,
        Logout,
        GameFinished,
        RequestUsers,
        NewProfile,
        RegisterError,
        LoginInfo,
        RequestPlayersTop,
        PlayersTop,
        RequestLastGames,
        LastGames
    }
}
