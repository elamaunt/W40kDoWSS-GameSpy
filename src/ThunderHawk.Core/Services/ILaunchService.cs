using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ThunderHawk.Core
{
    public interface ILaunchService
    {
        void SwitchGameToMod(string modName);
        string GetCurrentModName();
        string GamePath { get; }
        bool IsGamePreparingToStart { set; get; }
        Process GameProcess { get; }

        Task LaunchGameAndWait(String server, String mode = "Classic");
        bool TryGetOrChoosePath(out string path);
        void ChangeGamePath();
        void ActivateGameWindow();
    }
}
