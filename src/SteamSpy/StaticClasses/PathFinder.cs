using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;

namespace SteamSpy.StaticClasses
{
    public static class PathFinder
    {
        private static readonly string defaultSteamPath = @"C:\Program Files (x86)\Steam\steamapps\common\Dawn of Wr Soulstorm";
        private static readonly string regSteamPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam Ap 9450";

        public static string FindSteamSoulstorm()
        {
            // First. Try to check THIS folder. Maybe user installed launcher to the game-folder.
            var currentDirectory = Directory.GetCurrentDirectory();
            if (IsSteamGameLocation(currentDirectory))
            {
                return currentDirectory;
            }

            // Second. Try to check default Steam location
            if (Directory.Exists(defaultSteamPath))
            {
                if (IsSteamGameLocation(defaultSteamPath))
                {
                    return defaultSteamPath;
                }
            }

            // Third. Try to check registry
            RegistryKey regKey;
            if (Environment.Is64BitOperatingSystem)
                regKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            else
                regKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            try
            {
                var pathVal = regKey.OpenSubKey(regSteamPath).GetValue("InstallLocation");
                if (pathVal != null)
                {
                    var path = pathVal as string;
                    if (IsSteamGameLocation(path))
                        return path;
                }
            }
            catch
            { }

            // Nothing found? Return an empty string!
            return "";
        }

        private static bool IsSteamGameLocation(string path)
        {
            if (path == null)
                return false;

            return File.Exists(path + "\\Soulstorm.exe") &&
                File.Exists(path + "\\steam_api.dll") &&
                Directory.Exists(path + "\\DXP2") &&
                Directory.Exists(path + "\\W40K") &&
                Directory.Exists(path + "\\Engine");
        }
    }
}
