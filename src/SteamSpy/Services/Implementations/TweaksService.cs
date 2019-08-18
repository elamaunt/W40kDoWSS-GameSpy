using System.Collections.Generic;
using ThunderHawk.Core.Services;
using ThunderHawk.Tweaks;

namespace ThunderHawk
{
    public class TweaksService : ITweaksService
    {
        public List<ITweakService> Tweaks { get; } = new List<ITweakService>() { new UnlockerService() };
    }
}
