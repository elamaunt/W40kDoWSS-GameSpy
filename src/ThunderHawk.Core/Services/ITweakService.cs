namespace ThunderHawk.Core.Services
{
    public interface ITweak
    {
        void ApplyTweak(string gamePath);
        bool CheckTweak();
    }
}
