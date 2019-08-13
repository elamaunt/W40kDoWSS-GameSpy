using Framework;

namespace ThunderHawk.Core
{
    public static class CoreContext
    {
        public static ILangService LangService => Service<ILangService>.Get();
        public static INewsProvider NewsProvider => Service<INewsProvider>.Get();
    }
}
