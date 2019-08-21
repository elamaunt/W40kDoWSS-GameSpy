using ThunderHawk.Core;
using ThunderHawk.Core.Services;

namespace ThunderHawk.Tweaks
{
    public class GridHotkeys : ITweak
    {
        public string TweakTitle => Core.CoreContext.LangService.GetString("HotKeysTweakTitle");

        public string TweakDescription => Core.CoreContext.LangService.GetString("HotKeysTweakDescription");

        public bool IsRecommendedTweak { get; } = false;

        public bool CheckTweak()
        {
            return Core.CoreContext.OptionsService.DisableFog;
        }

        public void EnableTweak()
        {
            Core.CoreContext.OptionsService.DisableFog = true;
        }

        public void DisableTweak()
        {
            Core.CoreContext.OptionsService.DisableFog = false;
        }
    }
}
