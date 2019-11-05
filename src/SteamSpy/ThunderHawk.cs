using Binarysharp.MemoryManagement;
using Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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

       /* public static unsafe string GetCurrentModLabel()
        {
            var process = ProcessManager.GetGameProcess(); // Utils.ProcessHelper.

            //67168856 Ядро DOW 1.2.120, elamaunt Mod
            //294899500 ????????????? ?? elamaunt Mod

            //292671276 ????????????? ?? elamaunt Mod
            //397773416 Ядро DOW 1.2.120, elamaunt Mod
            
            //293785388 ????????????? ? а elamaunt Mod
            //397970024 Ядро DOW 1.2.120, elamaunt Mod
            //397970308 r: Soulstorm ???? elamaunt Mod

            if (process != null)
            {
                var pp = new MemorySharp(process);
                byte[] str = Encoding.Unicode.GetBytes("elamaunt Mod");

                 SystemInfo si;
                 GetSystemInfo(out si);

                 byte[] chunk = new byte[1024];

                 byte* p = (byte*)0;
                 var max = (byte*)si.MaximumApplicationAddress;
                 MEMORY_BASIC_INFORMATION info;

                int region = 0;

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
                                         var value = Encoding.Unicode.GetString(chunk, i - 36, str.Length + 36);
                                         var offset = p + i - 36;
                                         //var rp = pp[(IntPtr)offset, true];

                                        //pp.ReadString()

                                         Console.WriteLine(((int)offset) + " " + value);
                                     }
                                 }
                             }
                         }

                        Console.WriteLine("R"+ region++);
                        p += s;
                        Console.WriteLine("offset"+ (int)p);
                    }
                }

            }

            return "";
        }*/

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                PathFinder.Find();

                if (!args.IsNullOrEmpty() && args[0] == "-original")
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
                }

                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

                new App().Run();
                CoreContext.MasterServer.Disconnect();
            }
            catch(Exception ex)
            {
                File.WriteAllText("StartupException.ex", ex.GetLowestBaseException().ToString());
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
