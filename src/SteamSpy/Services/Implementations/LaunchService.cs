using Microsoft.Win32;
using Steamworks;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ThunderHawk.Core;
using ThunderHawk.StaticClasses.Soulstorm;
using ThunderHawk.Utils;

namespace ThunderHawk
{
    public class LaunchService : ILaunchService
    {
        public bool CanLaunchGame => !ProcessManager.GameIsRunning() && PathFinder.IsPathFound();

        public string GamePath => PathFinder.GamePath;

        public Process GameProcess { get; private set; }

        public string LauncherPath
        {
            get
            {
                if (Environment.CurrentDirectory != PathFinder.GamePath)
                    return Environment.CurrentDirectory;

                var regKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).OpenSubKey(ThunderHawk.RegistryKey);
                if (regKey != null)
                {
                    var pathKey = (string)regKey.GetValue("Path");
                    return pathKey;
                }

                return null;
            }
        }

        public Task LaunchThunderHawkGameAndWait()
        {
            return Task.Factory.StartNew(async () =>
            {
                ProcessManager.KillAllGameProccessesWithoutWindow();


                if (ProcessManager.GameIsRunning())
                   throw new Exception("Game is running");

                if (!PathFinder.IsPathFound())
                    throw new Exception("Path to game not found in Steam");

                IPHostEntry entry = null;


                try
                {
                    entry = Dns.GetHostEntry("gamespygp");
                }
                catch(Exception)
                {
                    FixHosts();
                }


                if (entry != null)
                {
                    var address = IPAddress.Parse(GameConstants.SERVER_ADDRESS);
                    if (!entry.AddressList.Any(x => x.Equals(address)))
                        FixHosts();
                }

                var tcs = new TaskCompletionSource<Process>();

                try
                {
                    try
                    {
                        Task.Factory.StartNew(() =>
                         {
                             while (!tcs.Task.IsCompleted)
                             {
                                 GameServer.RunCallbacks();
                                 SteamAPI.RunCallbacks();
                                 PortBindingManager.UpdateFrame();
                                 Thread.Sleep(5);
                             }
                         }, TaskCreationOptions.LongRunning);

                        var exeFileName = Path.Combine(LauncherPath, "GameFiles", "Patch1.2", "Soulstorm.exe");
                        var procParams = "-nomovies -forcehighpoly";
                        if (AppSettings.ThunderHawkModAutoSwitch)
                            procParams += " -modname ThunderHawk";

                        CopyHotKeysIfFirstLaunch(GamePath);

                        ProcessManager.KillDowStatsProccesses();


                        var ssProc = Process.Start(new ProcessStartInfo(exeFileName, procParams)
                        {
                            UseShellExecute = true,
                            WorkingDirectory = PathFinder.GamePath
                        });

                        GameProcess = ssProc;

                        CoreContext.ClientServer.Start();

                        ssProc.EnableRaisingEvents = true;

                        Task.Run(() => RemoveFogLoop(tcs.Task, ssProc));

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

                    await tcs.Task;
                }
                finally
                {
                    GameProcess = null;
                    CoreContext.ClientServer.Stop();
                    SteamLobbyManager.LeaveFromCurrentLobby();
                }
            }).Unwrap();
        }


        private void CopyHotKeysIfFirstLaunch(string gamePath)
        {
            var profiles = Directory.GetDirectories(Path.Combine(gamePath, "Profiles"));
            foreach (var profile in profiles)
            {
                var hotKeysPath = Path.Combine(profile, "dxp2", "KEYDEFAULTS.LUA");
                if (File.Exists(hotKeysPath))
                {
                    var targetDir = Path.Combine(profile, "thunderhawk");
                    var targetPath = Path.Combine(targetDir, "KEYDEFAULTS.LUA");
                    if (!File.Exists(targetPath))
                    {
                        if (!Directory.Exists(targetDir))
                            Directory.CreateDirectory(targetDir);

                        File.Copy(hotKeysPath, targetPath, true);
                    }
                }
            }
        }

        private void FixHosts()
        {
            var process = Process.Start("ThunderHawk.HostsFixer.exe", GameConstants.SERVER_ADDRESS);
            process.WaitForExit();

            if (process.ExitCode == 1)
                throw new Exception("Can not modify hosts file. You should modify HOSTS manually in C:\\Windows\\System32\\drivers\\etc. Use sample from discord https://discordapp.com/invite/Tfgf3yd");
        }

        private async Task RemoveFogLoop(Task task, Process ssProc)
        {
            while (!task.IsCompleted)
            {
                if (!AppSettings.DisableFog || FogRemover.DisableFog(ssProc))
                {
                    return;
                }
                await Task.Delay(1000);
            }
        }

        public void LaunchOriginalGame()
        {
            ProcessManager.KillDowStatsProccesses();

            Process.Start(new ProcessStartInfo(Path.Combine(LauncherPath, "ThunderHawk.exe"), "-original")
            {
                UseShellExecute = true,
                WorkingDirectory = LauncherPath
            });

            Environment.Exit(0);
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
