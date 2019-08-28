﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using ThunderHawk.Core;
using ThunderHawk.StaticClasses.Soulstorm;

namespace ThunderHawk
{
    public class LaunchService : ILaunchService
    {
        public bool CanLaunchGame => !ProcessManager.GameIsRunning() && PathFinder.IsPathFound();

        public string GamePath => PathFinder.GamePath;

        public Task LaunchGameAndWait()
        {
            if (!CanLaunchGame)
                throw new Exception();

            var tcs = new TaskCompletionSource<Process>();

            try
            {
                var exeFileName = Path.Combine(Directory.GetCurrentDirectory(), "GameFiles", "Patch1.2", "Soulstorm.exe");
                var procParams = "-nomovies -forcehighpoly";
                if (AppSettings.ThunderHawkModAutoSwitch)
                    procParams += " -modname ThunderHawk";
                var ssProc = Process.Start(new ProcessStartInfo(exeFileName, procParams)
                {
                    UseShellExecute = true,
                    WorkingDirectory = PathFinder.GamePath
                });

                Task.Run(() => RemoveFogLoop(ssProc));

                ssProc.Exited += (s, e) =>
                {
                    tcs.TrySetResult(ssProc);
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }

        private async Task RemoveFogLoop(Process ssProc)
        {
            while (true)
            {
                if (!AppSettings.DisableFog || FogRemover.DisableFog(ssProc))
                {
                    return;
                }
                await Task.Delay(1000);
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
