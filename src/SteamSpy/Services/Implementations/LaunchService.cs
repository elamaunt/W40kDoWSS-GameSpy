﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
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
                    //FogRemover.DisableFog(ssProc)
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
