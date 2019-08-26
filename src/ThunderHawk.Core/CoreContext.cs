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
        public static ILaunchService LaunchService => Service<ILaunchService>.Get();
        public static ITweaksService TweaksService => Service<ITweaksService>.Get();
        public static IThunderHawkModManager ThunderHawkModManager => Service<IThunderHawkModManager>.Get();
    }
}
