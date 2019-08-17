using System;
using System.Diagnostics;
using System.IO;
using ThunderHawk.Core;
using ThunderHawk.StaticClasses.Soulstorm;
using ThunderHawk.Tweaks;

namespace ThunderHawk
{
    public class LaunchService : ILaunchService
    {
        private bool CanLaunchGame()
        {
            return !ProcessFinder.IsProcessFound() && PathFinder.IsPathFound();
        }

        public void LaunchGame()
        {
            if (!CanLaunchGame())
                return;

            try
            {
                var exeFileName = PathFinder.GamePath + "\\LauncherFiles\\Patch1.2\\soulstorm.exe";
                if (File.Exists(exeFileName))
                {
                    var ssProc = Process.Start(new ProcessStartInfo(exeFileName, $"-nomovies")
                    {
                        UseShellExecute = true,
                        WorkingDirectory = PathFinder.GamePath
                    });
                    if (Core.CoreContext.OptionsService.DisableFog)
                        FogRemover.DisableFog(ssProc);
                }
                else
                {
                    throw new Exception("No soulstorm.exe in patch12 folder found!");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
