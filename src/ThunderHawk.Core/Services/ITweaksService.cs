using System.Collections.Generic;

namespace ThunderHawk.Core.Services
{
    public interface ITweaksService
    {
        ITweak Unlocker { get; }

        TweaksState GetState();

        ITweak[] GetWrongTweaks();
    }
}
