using System;
using ThunderHawk.Core;
using ThunderHawk.Core.Services;

namespace ThunderHawk.Tweaks
{
    public class FogSwitcher : ITweak
    {
        public string TweakTitle => Core.CoreContext.LangService.GetString("FogTweakTitle");

        public string TweakDescription => Core.CoreContext.LangService.GetString("FogTweakDescription");

        public TweakLevel TweakLevel { get; } = TweakLevel.Normal;

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
