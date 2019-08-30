using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;

namespace ThunderHawk.StaticClasses.Soulstorm
{
    public static class PathFinder
    {
        private static readonly string defaultSteamPath = @"C:\Program Files (x86)\Steam\steamapps\common\Dawn of War Soulstorm";
        private static readonly string SoulstormSteamRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 9450";
        private static readonly string SteamRegistryPath = @"SOFTWARE\Valve\Steam";
        private static readonly string SteamRegistry64Path = @"SOFTWARE\Wow6432Node\Valve\Steam";
        
        public static string GamePath { get; private set; }

        public static bool IsPathFound()
        {
            return !string.IsNullOrWhiteSpace(GamePath);
        }

        public static void Find()
        {
            GamePath = FindSteamSoulstorm();
        }

        private static string FindSteamSoulstorm()
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
                var pathVal = regKey.OpenSubKey(SoulstormSteamRegistryPath).GetValue("InstallLocation");
                if (pathVal != null)
                {
                    var path = pathVal as string;
                    if (IsSteamGameLocation(path))
                        return path;
                }
            }
            catch
            {
            }

            var steamPath = FindSteam();

            if (steamPath == null)
                return null;

            try
            {
                var vdfsLines = File.ReadAllLines(Path.Combine(steamPath, "steamapps", "libraryfolders.vdf"));

                for (int i = 0; i < vdfsLines.Length; i++)
                {
                    var line = vdfsLines[i];

                    if (line.StartsWith("\""))
                        continue;
                    if (line.StartsWith("{"))
                        continue;
                    if (line.StartsWith("}"))
                        continue;

                    var split = line.Split('"').Where(x => !x.IsNullOrWhiteSpace()).ToArray();

                    if (int.TryParse(split[0], out int key))
                    {
                        var path = Path.Combine(split[1], "steamapps", "common");

                        foreach (var directory in Directory.EnumerateDirectories(path))
                        {
                            if (directory.IndexOf("soulstorm", StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                if (IsSteamGameLocation(directory))
                                    return directory;
                            }
                        }
                        
                    }
                }
            }
            catch
            {

            }

            return null;
        }

        private static string FindSteam()
        {
            //32 - bit: HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam
            //64 - bit: HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam

            RegistryKey regKey;
            if (Environment.Is64BitOperatingSystem)
                regKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            else
                regKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);

            try
            {
                var pathVal = regKey.OpenSubKey(SteamRegistryPath).GetValue("InstallPath");
                if (pathVal != null && pathVal is string)
                {
                    var path = (string)pathVal;
                    if (File.Exists(Path.Combine(path, "Steam.exe")))
                        return path;
                }
            }
            catch
            {
            }

            try
            {
                var pathVal = regKey.OpenSubKey(SteamRegistry64Path).GetValue("InstallPath");
                if (pathVal != null && pathVal is string)
                {
                    var path = (string)pathVal;
                    if (File.Exists(Path.Combine(path, "Steam.exe")))
                        return path;
                }
            }
            catch
            {
            }

            return null;
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
