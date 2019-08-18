using System.Collections.Generic;
using ThunderHawk.Core.Services;
using ThunderHawk.Tweaks;

namespace ThunderHawk
{
    public class TweaksService : ITweaksService
    {
        public ITweakService Unlocker { get; } = new UnlockerService();
    }
}
