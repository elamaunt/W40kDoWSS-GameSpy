using Framework;
using ThunderHawk.Core.Services;

namespace ThunderHawk.Core
{
    public static class CoreContext
    {
        public static IHttpService HttpService => Service<IHttpService>.Get();
        public static ISystemService SystemService => Service<ISystemService>.Get();
        public static ILangService LangService => Service<ILangService>.Get();
        public static INewsProvider NewsProvider => Service<INewsProvider>.Get();
        public static IAccountService AccountService => Service<IAccountService>.Get();
        public static ILaunchService LaunchService => Service<ILaunchService>.Get();
        public static ITweaksService TweaksService => Service<ITweaksService>.Get();

        public static ISteamApiService SteamApi => Service<ISteamApiService>.Get();
        public static IUpdaterService UpdaterService => Service<IUpdaterService>.Get();
        public static IMasterServer MasterServer => Service<IMasterServer>.Get();
        public static IClientServer ClientServer => Service<IClientServer>.Get();
        public static IResourcesService ResourcesService => Service<IResourcesService>.Get();
        public static IOpenLogsService OpenLogsService => Service<IOpenLogsService>.Get();
        public static IInGameService InGameService => Service<IInGameService>.Get();
        
        public static IThunderHawkModManager ThunderHawkModManager => Service<IThunderHawkModManager>.Get();
    }
}
