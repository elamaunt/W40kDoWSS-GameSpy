using Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using ThunderHawk.Core;
using ThunderHawk.StaticClasses.Soulstorm;

namespace ThunderHawk
{
    public static class ThunderHawk
    {
        public const string RegistryKey = "SoftWare\\ThunderHawk";
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

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                PathFinder.Find();

                bool silence = false;

                if (!args.IsNullOrEmpty() && args[0] == "-silence")
                    silence = true;

                /*if (!args.IsNullOrEmpty() && args[0] == "-original")
                {
                    Thread.Sleep(5000);
                    File.Copy(Path.Combine(Environment.CurrentDirectory, "SoulstormBackup", "Soulstorm.exe" ), Path.Combine(PathFinder.GamePath, "Soulstorm.exe"), true);
                    Thread.Sleep(1000);
                    Process.Start(new ProcessStartInfo(Path.Combine(PathFinder.GamePath, "Soulstorm.exe"), "-nomovies -forcehighpoly -modname dxp2")
                    {
                        UseShellExecute = true,
                        WorkingDirectory = PathFinder.GamePath
                    });

                    Environment.Exit(0);
                    return;
                }*/

                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

                new App(silence).Run();
                CoreContext.MasterServer.Disconnect();
            }
            catch(Exception ex)
            {
                ex = ex.GetLowestBaseException();

                var text = $"{ex.Message} {ex.StackTrace}";

                File.WriteAllText("StartupException.ex", text);

                MessageBox.Show(text);
                throw;
            }
        }

        static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            File.WriteAllText("LastException.ex", (e.ExceptionObject as Exception).GetLowestBaseException().ToString());
        }



        /*static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            Append(args.Name);

            if (File.Exists(args.Name))
                return null;

            var path = Path.Combine(LauncherInstalledPath, args.Name);

            if (File.Exists(path))
                return Assembly.LoadFrom(path);

            return null;
        }

        private static void Append(string text)
        {
            File.AppendAllText("Resolving.ex", "\n" + text);
        }*/
    }
}
