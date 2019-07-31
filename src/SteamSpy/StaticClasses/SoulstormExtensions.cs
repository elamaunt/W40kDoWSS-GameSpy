using SteamSpy.StaticClasses.DataKeepers;
using System;
using System.Diagnostics;
using System.Linq;

namespace SteamSpy.StaticClasses
{
    public static class SoulstormExtensions
    {
        public static Process[] FindGameProcesses()
        {
            return Process.GetProcessesByName("Soulstorm");
        }
        public static bool IsGameRunning()
        {
            return FindGameProcesses().Length > 0;
        }

        public static bool IsGameInstalled()
        {
            return !string.IsNullOrWhiteSpace(RunTimeData.GamePath);
        }

        public static bool CanLaunchGame()
        {
            return !IsGameRunning() && IsGameInstalled();
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
                //TODO: Nlogger log(ex)...
            }
        }
    }
}
