namespace ThunderHawk.Core.Services
{
    public interface ITweak
    {
        string TweakTitle { get; }
        string TweakDescription { get; }
        bool IsRecommendedTweak { get; }
        void EnableTweak();
        void DisableTweak();
        bool CheckTweak();
    }
}
