using NLog;
using SteamSpy.Tweaks;
using System;
using System.Diagnostics;
using System.IO;

namespace SteamSpy.StaticClasses
{
    public static class SoulstormExtensions
    {
        private static string GamePath { get; set; }

        public static void Init()
        {
            GamePath = PathFinder.FindSteamSoulstorm();
        }

        public static Process[] FindGameProcesses()
        {
            return Process.GetProcessesByName("Soulstorm");
        }
        public static bool IsGameRunning()
        {
            return FindGameProcesses().Length > 0;
        }

        public static bool IsPathFound()
        {
            return !string.IsNullOrWhiteSpace(GamePath);
        }

        public static bool CanLaunchGame()
        {
            return !IsGameRunning() && IsPathFound();
        }

        public static void LaunchGame()
        {
            if (!CanLaunchGame())
                return;

            try
            {
                var unlocker = new UnlockRacesTweak(GamePath);
                if (!unlocker.IsTweakApplied())
                {
                    unlocker.ApplyTweak();
                }
                var exeFileName = GamePath + "\\LauncherFiles\\Patch1.2\\soulstorm.exe";
                if (File.Exists(exeFileName))
                {
                    Process.Start(new ProcessStartInfo(exeFileName, $"-nomovies")
                    {
                        UseShellExecute = true,
                        WorkingDirectory = GamePath
                    });
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
