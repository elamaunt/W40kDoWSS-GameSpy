using System.Collections.Generic;
using System.Linq;
using ThunderHawk.Core;
using ThunderHawk.Core.Services;
using ThunderHawk.Tweaks;

namespace ThunderHawk
{
    public class TweaksService : ITweaksService
    {
        public ITweak Unlocker { get; } = new Unlocker();

        public TweakState GetState()
        {
            var unSuccessfulTweaks = GetWrongTweaks();
            if (unSuccessfulTweaks.Length > 0)
            {
                if (unSuccessfulTweaks.Any(x => x.TweakLevel == TweakState.Error))
                {
                    return TweakState.Error;
                }
                else if (unSuccessfulTweaks.Any(x => x.TweakLevel == TweakState.Warning))
                {
                    return TweakState.Warning;
                }
            }
            return TweakState.Success;
        }

        public ITweak[] GetWrongTweaks()
        {
            var retTweaks = new List<ITweak>();

            if (!Unlocker.CheckTweak())
                retTweaks.Add(Unlocker);

            return retTweaks.ToArray();
        }
    }
}
