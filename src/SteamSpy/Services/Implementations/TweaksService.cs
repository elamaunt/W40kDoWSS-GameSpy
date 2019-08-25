using System.Collections.Generic;
using System.Linq;
using ThunderHawk.Core;
using ThunderHawk.Core.Services;
using ThunderHawk.Tweaks;

namespace ThunderHawk
{
    public class TweaksService : ITweaksService
    {
        public ITweak[] Tweaks { get; } = new ITweak[]
            { new Unlocker(), new Camera(), new RuFont(),
              new FogSwitcher(), new GridHotkeys()  };

        public bool GetState()
        {
            var disabledTweaks = GetDisabledTweaks();
            if (disabledTweaks.Length > 0)
            {
                if (disabledTweaks.Any(x => x.IsRecommendedTweak))
                {
                    return true;
                }
            }
            return false;
        }

        private ITweak[] GetDisabledTweaks()
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
