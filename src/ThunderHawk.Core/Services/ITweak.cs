namespace ThunderHawk.Core.Services
{
    public interface ITweak
    {
        string TweakTitle { get; }
        string TweakDescription { get; }
        TweakLevel TweakLevel { get; }
        void EnableTweak();
        void DisableTweak();
        bool CheckTweak();
    }
}
