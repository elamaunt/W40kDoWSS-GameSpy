using System.Collections.Generic;

namespace ThunderHawk.Core.Services
{
    public interface ITweaksService
    {
        List<ITweakService> Tweaks { get; }
    }
}
