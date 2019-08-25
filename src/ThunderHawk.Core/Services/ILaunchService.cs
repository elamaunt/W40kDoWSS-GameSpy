using System.Threading.Tasks;

namespace ThunderHawk.Core
{
    public interface ILaunchService
    {
        void SwitchGameToMod(string modName);
        string GamePath { get; }
        bool CanLaunchGame { get; }
        Task LaunchGameAndWait();
    }
}
