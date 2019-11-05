using System.Diagnostics;
using System.Threading.Tasks;

namespace ThunderHawk.Core
{
    public interface ILaunchService
    {
        void SwitchGameToMod(string modName);
        string GetCurrentModName();
        string GamePath { get; }
        bool CanLaunchGame { get; }
        Process GameProcess { get; }

        Task LaunchThunderHawkGameAndWait();
        bool TryGetOrChoosePath(out string path);
        void ChangeGamePath();
    }
}
