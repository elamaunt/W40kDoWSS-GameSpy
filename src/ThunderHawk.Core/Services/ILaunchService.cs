using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Framework;

namespace ThunderHawk.Core
{
    public interface ILaunchService
    {
        void SwitchGameToMod(string modName);
        string GetCurrentModName();
        string GamePath { get; }
        double PreparingProgress { set; get; }
        string PreparingModName { set; get; }
        bool IsGamePreparingToStart { set; get; }
        Process GameProcess { get; }

        Task LaunchGameAndWait(String server, String mode, IGlobalNavigationManager globalNavigationManager);
        bool TryGetOrChoosePath(out string path);
        void ChangeGamePath();
        void ActivateGameWindow();
    }
}
