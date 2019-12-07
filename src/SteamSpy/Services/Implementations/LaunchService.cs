using Steamworks;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
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

        /*public string LauncherPath
        {
            get
            {
                if (Environment.CurrentDirectory != PathFinder.GamePath)
                    return Environment.CurrentDirectory;

                var regKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).OpenSubKey(ThunderHawk.RegistryKey);
                var pathKey = (string) regKey?.GetValue("Path");
                return pathKey;
            }
        }*/

        public Task LaunchThunderHawkGameAndWait()
        {
            if (!CoreContext.LaunchService.TryGetOrChoosePath(out string path))
                return Task.CompletedTask;

            return Task.Factory.StartNew(async () =>
            {
                ProcessManager.KillAllGameProccessesWithoutWindow();

                if (!CoreContext.MasterServer.IsConnected)
                    throw new Exception("Server is unavailable");

                if (ProcessManager.GameIsRunning())
                    throw new Exception("Game is running");

                if (!PathFinder.IsPathFound())
                    throw new Exception("Path to game not found in Steam");

                CoreContext.ThunderHawkModManager.DeployModAndModule(path);

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
                    var address = NetworkHelper.GetLocalIpAddresses().FirstOrDefault() ?? IPAddress.Loopback;
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

                        var exeFileName = Path.Combine(Environment.CurrentDirectory,  "GameFiles", "Patch1.2", "Soulstorm.exe");
                        var procParams = "-nomovies -forcehighpoly";
                        if (AppSettings.ThunderHawkModAutoSwitch)
                            procParams += " -modname ThunderHawk";

                        CopySchemes(path);
                        CopyHotkeys(path);

                        ProcessManager.KillDowStatsProccesses();

                        var ssProc = Process.Start(new ProcessStartInfo(exeFileName, procParams)
                        {
                            UseShellExecute = true,
                            WorkingDirectory = path
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

        private void CopySchemes(string gamePath)
        {
            var profiles = Directory.GetDirectories(Path.Combine(gamePath, "Profiles"));
            foreach (var profile in profiles)
            {
                var schemesPath = Path.Combine(profile, "dxp2", "Schemes");
                if (Directory.Exists(schemesPath))
                {
                    var files = Directory.GetFiles(schemesPath, "*.teamcolour");
                    if (files.Length == 0)
                        continue;
                    var targetDir = Path.Combine(profile, "thunderhawk", "Schemes");
                    if (!Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                        foreach(var file in files)
                        {
                            File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
                        }
                    }
                }
            }
        }
        void CopyHotkeys(string gamePath)
        {
            var profiles = Directory.GetDirectories(Path.Combine(gamePath, "Profiles"));
            foreach (var profile in profiles)
            {
                var hotKeysPath = Path.Combine(profile, "dxp2", "KEYDEFAULTS.LUA");
                if (File.Exists(hotKeysPath))
                {
                    var targetDir = Path.Combine(profile, "thunderhawk");

                    if (!Directory.Exists(targetDir))
                        Directory.CreateDirectory(targetDir);

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

        void FixHosts()
        {
            var addresses = NetworkHelper.GetLocalIpAddresses();
            var address = addresses.LastOrDefault() ?? IPAddress.Loopback;

            var process = Process.Start("ThunderHawk.HostsFixer.exe", address.ToString());
            process.WaitForExit();

            if (process.ExitCode == 1)
                throw new Exception("Can not modify hosts file. You should modify HOSTS manually in C:\\Windows\\System32\\drivers\\etc. Use sample from discord https://discordapp.com/invite/Tfgf3yd");
        }

        async Task RemoveFogLoop(Task task, Process ssProc)
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

        public bool TryGetOrChoosePath(out string path)
        {
            return PathFinder.TryGetOrChoosePath(out path);
        }

        public void ChangeGamePath()
        {
            PathFinder.ChoosePath();
        }

        public void SwitchGameToMod(string modName)
        {
            try
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
            }
            catch(Exception ex)
            {

            }
        }

        public enum ProcessorArchitecture
        {
            X86 = 0,
            X64 = 9,
            @Arm = -1,
            Itanium = 6,
            Unknown = 0xFFFF,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SystemInfo
        {
            public ProcessorArchitecture ProcessorArchitecture; // WORD
            public uint PageSize; // DWORD
            public IntPtr MinimumApplicationAddress; // (long)void*
            public IntPtr MaximumApplicationAddress; // (long)void*
            public IntPtr ActiveProcessorMask; // DWORD*
            public uint NumberOfProcessors; // DWORD (WTF)
            public uint ProcessorType; // DWORD
            public uint AllocationGranularity; // DWORD
            public ushort ProcessorLevel; // WORD
            public ushort ProcessorRevision; // WORD
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        [DllImport("kernel32.dll")]
        static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [DllImport("kernel32.dll", SetLastError = false)]
        public static extern void GetSystemInfo(out SystemInfo Info);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);
       
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern unsafe int memcmp(byte* b1, byte* b2, int count);

        public static unsafe int CompareBuffers(byte[] buffer1, int offset1, byte[] buffer2, int offset2, int count)
        {
            fixed (byte* b1 = buffer1, b2 = buffer2)
            {
                return memcmp(b1 + offset1, b2 + offset2, count);
            }
        }

        public unsafe string GetCurrentModName()
        {
            // TODO: не работает
            var process = GameProcess;

            byte[] str = Encoding.ASCII.GetBytes("thunderhawk");

            if (process != null)
            {
                SystemInfo si;
                GetSystemInfo(out si);

                byte[] chunk = new byte[1024];

                byte* p = (byte*)0;
                var max = (byte*)si.MaximumApplicationAddress;
                MEMORY_BASIC_INFORMATION info;

                while (p < max)
                {
                    if (VirtualQueryEx(process.Handle, (IntPtr)p, out info, (uint)sizeof(MEMORY_BASIC_INFORMATION)) == sizeof(MEMORY_BASIC_INFORMATION))
                    {
                        p = (byte*)info.BaseAddress;
                        //chunk.resize(info.RegionSize);

                        //var s = Marshal.SizeOf(info.RegionSize);
                        var s = (int)info.RegionSize;
                        if (chunk.Length < s)
                            chunk = new byte[s];

                        IntPtr bytesRead;
                        if (ReadProcessMemory(process.Handle, (IntPtr)p, chunk, s, out bytesRead))
                        {
                            var rs = (int)bytesRead;

                            if (rs >= str.Length)
                            {
                                for (int i = 0; i < (rs - str.Length); ++i)
                                {
                                    if (CompareBuffers(str, 0, chunk, i, str.Length) == 0)
                                    {
                                        var value = Encoding.ASCII.GetString(chunk, i-100, str.Length + 100);
                                        Console.WriteLine("F "+ value);
                                    }
                                }
                            }
                        }
                        p += s;
                    }
                }
            }


            var localConfig = Path.Combine(GamePath, "Local.ini");

            if (!File.Exists(localConfig))
                return null;

            var lines = File.ReadAllLines(localConfig);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("currentmoddc=", StringComparison.OrdinalIgnoreCase))
                    return lines[i].Substring(13).TrimEnd();
            }

            return null;
        }
    }
}
