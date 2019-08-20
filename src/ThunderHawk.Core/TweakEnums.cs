using System;
using System.Collections.Generic;
using System.Text;

namespace ThunderHawk.Core
{
    public enum TweaksState
    {
        Success,
        Warning,
        Error
    }

    public enum TweakLevel : byte
    {
        Normal,
        Recommended,
        Important
    }
}
