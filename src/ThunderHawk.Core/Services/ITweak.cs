﻿namespace ThunderHawk.Core.Services
{
    public interface ITweak
    {
        void ApplyTweak();
        bool CheckTweak();
        string Title { get;}
        string Description { get; }
        TweakState TweakLevel { get; }
    }
}
