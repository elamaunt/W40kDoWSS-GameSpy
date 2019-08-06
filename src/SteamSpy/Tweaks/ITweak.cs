using System.Threading.Tasks;

namespace SteamSpy.Tweaks
{
    public interface ITweak
    {
        Task ApplyTweak();
        bool IsTweakApplied();
    }
}
