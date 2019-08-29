namespace ThunderHawk.Core
{
    public interface ISteamApiService
    {
        void Initialize();

        bool IsInitialized { get; }

        string NickName { get; }
    }
}
