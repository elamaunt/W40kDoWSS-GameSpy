using ThunderHawk.Tweaks;
using System;
using System.Diagnostics;
using System.IO;

namespace ThunderHawk.StaticClasses.Soulstorm
{
    public static class Launcher
    {
        public static bool CanLaunchGame()
        {
            return !ProcessFinder.IsProcessFound() && PathFinder.IsPathFound();
        }

        public static void LaunchGame()
        {
            if (!CanLaunchGame())
                return;

            try
            {
                var unlocker = new UnlockRacesTweak(PathFinder.GamePath);
                if (!unlocker.IsTweakApplied())
                {
                    unlocker.ApplyTweak();
                }
                var exeFileName = PathFinder.GamePath + "\\LauncherFiles\\Patch1.2\\soulstorm.exe";
                if (File.Exists(exeFileName))
                {
                    var ssProc = Process.Start(new ProcessStartInfo(exeFileName, $"-nomovies")
                    {
                        UseShellExecute = true,
                        WorkingDirectory = PathFinder.GamePath
                    });
                    if (FogRemover.DisableFog(ssProc))
                    {
                        
                    }
                    else
                    {

                    }
                }
                else
                {
                    throw new Exception("No soulstorm.exe in patch12 folder found!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
