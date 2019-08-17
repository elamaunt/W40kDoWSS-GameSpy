using Framework;
using ThunderHawk.Core.Services;

namespace ThunderHawk.Core
{
    public static class CoreContext
    {
        public static ILangService LangService => Service<ILangService>.Get();
        public static INewsProvider NewsProvider => Service<INewsProvider>.Get();
        public static IOptionsService OptionsService => Service<IOptionsService>.Get();
        public static ILaunchService LaunchService => Service<ILaunchService>.Get();
        public static ITweaksService TweaksService => Service<ITweaksService>.Get();

    }
}
