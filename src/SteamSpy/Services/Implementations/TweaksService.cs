using System.Collections.Generic;
using ThunderHawk.Core.Services;
using ThunderHawk.Tweaks;

namespace ThunderHawk
{
    public class TweaksService : ITweaksService
    {
        public ITweak Unlocker { get; } = new Unlocker();
    }
}
