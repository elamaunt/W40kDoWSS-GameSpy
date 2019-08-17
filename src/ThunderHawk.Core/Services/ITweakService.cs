namespace ThunderHawk.Core.Services
{
    public interface ITweakService
    {
        void ApplyTweak(string gamePath);
        bool CheckTweak();
    }
}
