using ThunderHawk.Core;
using ThunderHawk.Core.Services;

namespace ThunderHawk.Tweaks
{
    public class GridHotkeys : ITweak
    {
        public string TweakTitle => Core.CoreContext.LangService.GetString("HotKeysTweakTitle");

        public string TweakDescription => Core.CoreContext.LangService.GetString("HotKeysTweakDescription");

        public bool IsRecommendedTweak { get; } = false;

        private bool switcher = false;
        public bool CheckTweak()
        {
            //TODO: Implement tweak logic
            return switcher;
        }

        public void EnableTweak()
        {
            switcher = true;
        }

        public void DisableTweak()
        {
            switcher = false;
        }
    }
}
