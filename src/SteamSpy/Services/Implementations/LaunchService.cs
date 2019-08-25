using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using ThunderHawk.Core;
using ThunderHawk.StaticClasses.Soulstorm;

namespace ThunderHawk
{
    public class LaunchService : ILaunchService
    {
        public bool CanLaunchGame => !ProcessFinder.IsProcessFound() && PathFinder.IsPathFound();

        public string GamePath => PathFinder.GamePath;

        public void LaunchGame()
        {
            if (!CanLaunchGame)
                return;

            try
            {
                var exeFileName = Path.Combine(Directory.GetCurrentDirectory(), "LauncherFiles", "Addons", "Patch1.2", "Soulstorm.exe");
                var procParams = "-nomovies -forcehighpoly";
                if (AppSettings.ThunderHawkModAutoSwitch)
                    procParams += " -modname ThunderHawk";
                var ssProc = Process.Start(new ProcessStartInfo(exeFileName, procParams)
                {
                    UseShellExecute = true,
                    WorkingDirectory = PathFinder.GamePath
                });

                if (AppSettings.DisableFog)
                    FogRemover.DisableFog(ssProc);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /*public void SwitchGameToMod(string modName)
        {
            var localConfig = Path.Combine(GamePath, "Local.ini");

            if (!File.Exists(localConfig))
                return;

            var lines = File.ReadAllLines(localConfig);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("currentmoddc=", StringComparison.OrdinalIgnoreCase))
                    lines[i] = "currentmoddc=thunderhawk";
            }

            File.WriteAllLines(localConfig, lines);
        }*/
    }
}
