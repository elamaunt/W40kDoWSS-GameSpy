using System;
using ThunderHawk.Core;
using ThunderHawk.Core.Services;
using ThunderHawk.StaticClasses.Soulstorm;

namespace ThunderHawk.Tweaks
{
    public class FogSwitcher : ITweak
    {
        public string TweakTitle => Core.CoreContext.LangService.GetString("FogTweakTitle");

        public string TweakDescription => Core.CoreContext.LangService.GetString("FogTweakDescription");

        public bool IsRecommendedTweak { get; } = false;

        public bool CheckTweak()
        {
            return AppSettings.DisableFog;
        }

        public void EnableTweak()
        {
            AppSettings.DisableFog = true;
            var proc = ProcessManager.GetGameProcess();
            if (proc != null)
            {
                FogRemover.DisableFog(proc);
            }
        }

        public void DisableTweak()
        {
            AppSettings.DisableFog = false;
            var proc = ProcessManager.GetGameProcess();
            if (proc != null)
            {
                FogRemover.EnableFog(proc);
            }
        }
    }
}
