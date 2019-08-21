using System.Collections.Generic;
using System.Linq;
using ThunderHawk.Core;
using ThunderHawk.Core.Services;
using ThunderHawk.Tweaks;

namespace ThunderHawk
{
    public class TweaksService : ITweaksService
    {
        public ITweak[] Tweaks { get; } = new ITweak[] { new Unlocker(), new FogSwitcher() };

        public TweaksState GetState()
        {
            var unSuccessfulTweaks = GetFailedTweaks();
            if (unSuccessfulTweaks.Length > 0)
            {
                if (unSuccessfulTweaks.Any(x => x.TweakLevel == TweakLevel.Important))
                {
                    return TweaksState.Error;
                }
                else if (unSuccessfulTweaks.Any(x => x.TweakLevel == TweakLevel.Recommended))
                {
                    return TweaksState.Warning;
                }
            }
            return TweaksState.Success;
        }

        private ITweak[] GetFailedTweaks()
        {
            var retTweaks = new List<ITweak>();

            foreach(var tweak in Tweaks)
            {
                if (!tweak.CheckTweak())
                    retTweaks.Add(tweak);
            }

            return retTweaks.ToArray();
        }
    }
}
