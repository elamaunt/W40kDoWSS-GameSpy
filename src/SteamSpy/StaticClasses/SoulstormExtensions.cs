using NLog;
using SteamSpy.StaticClasses.DataKeepers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SteamSpy.StaticClasses
{
    public static class SoulstormExtensions
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
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
            return !string.IsNullOrWhiteSpace(RunTimeData.GamePath);
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
                var exeFileName = RunTimeData.GamePath + "\\LauncherFiles\\Patch1.2\\soulstorm.exe";
                if (File.Exists(exeFileName))
                {
                    Process.Start(new ProcessStartInfo(exeFileName, $"-nomovies")
                    {
                        UseShellExecute = true,
                        WorkingDirectory = RunTimeData.GamePath
                    });
                }
                else
                {
                    throw new Exception("No soulstorm.exe in patch12 folder found!");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}
