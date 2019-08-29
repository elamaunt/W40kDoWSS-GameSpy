
namespace ThunderHawk.Core.Services
{
    public interface ITweaksService
    {
        ITweak[] Tweaks { get; }

        bool RecommendedTweaksExists();
    }
}
