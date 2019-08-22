using ThunderHawk.Core.Services;

namespace ThunderHawk.Tweaks
{
    public class Camera : ITweak
    {
        public string TweakTitle => Core.CoreContext.LangService.GetString("CameraTweakTitle");

        public string TweakDescription => Core.CoreContext.LangService.GetString("CameraTweakDescription");

        public bool IsRecommendedTweak { get; } = true;

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
