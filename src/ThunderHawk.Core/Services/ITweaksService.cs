using System.Collections.Generic;

namespace ThunderHawk.Core.Services
{
    public interface ITweaksService
    {
        ITweak[] Tweaks { get; }

        TweaksState GetState();
    }
}
