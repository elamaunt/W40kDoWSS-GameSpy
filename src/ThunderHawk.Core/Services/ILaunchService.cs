using System.Diagnostics;
using System.Threading.Tasks;

namespace ThunderHawk.Core
{
    public interface ILaunchService
    {
        //void SwitchGameToMod(string modName);
        string GamePath { get; }
        string LauncherPath { get; }
        bool CanLaunchGame { get; }
        Process GameProcess { get; }

        Task LaunchThunderHawkGameAndWait();
        void LaunchOriginalGame();
    }
}
