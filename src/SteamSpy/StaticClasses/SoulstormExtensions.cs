using NLog;
using SteamSpy.StaticClasses.DataKeepers;
using System;
using System.Diagnostics;
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
                var exeFileName = RunTimeData.GamePath + "\\soulstorm.exe";
                Process.Start(new ProcessStartInfo(exeFileName, $"-nomovies")
                {
                    UseShellExecute = true,
                    WorkingDirectory = RunTimeData.GamePath
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}
