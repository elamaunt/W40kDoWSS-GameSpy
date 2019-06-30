using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace DllHook
{
    [Flags]
    public enum ProcessAccessFlags : uint
    {
        All = 0x001F0FFF,
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VirtualMemoryOperation = 0x00000008,
        VirtualMemoryRead = 0x00000010,
        VirtualMemoryWrite = 0x00000020,
        DuplicateHandle = 0x00000040,
        CreateProcess = 0x000000080,
        SetQuota = 0x00000100,
        SetInformation = 0x00000200,
        QueryInformation = 0x00000400,
        QueryLimitedInformation = 0x00001000,
        Synchronize = 0x00100000
    }

    [Flags]
    public enum AllocationType
    {
        Commit = 0x1000,
        Reserve = 0x2000,
        Decommit = 0x4000,
        Release = 0x8000,
        Reset = 0x80000,
        Physical = 0x400000,
        TopDown = 0x100000,
        WriteWatch = 0x200000,
        LargePages = 0x20000000
    }

    [Flags]
    public enum MemoryProtection
    {
        Execute = 0x10,
        ExecuteRead = 0x20,
        ExecuteReadWrite = 0x40,
        ExecuteWriteCopy = 0x80,
        NoAccess = 0x01,
        ReadOnly = 0x02,
        ReadWrite = 0x04,
        WriteCopy = 0x08,
        GuardModifierflag = 0x100,
        NoCacheModifierflag = 0x200,
        WriteCombineModifierflag = 0x400
    }

    public class Exporter
    {
        /// <summary>
        /// Opens an existing local process object.
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Int32 CloseHandle(IntPtr hObject);
    }
    public class Injector
    {
        public static void Inject(Int32 pid, String dllPath)
        {
            IntPtr openedProcess = Exporter.OpenProcess(ProcessAccessFlags.All, false, pid);
            IntPtr kernelModule = Exporter.GetModuleHandle("kernel32.dll");
            IntPtr loadLibraryAddr = Exporter.GetProcAddress(kernelModule, "LoadLibraryA");

            Int32 len = dllPath.Length;
            IntPtr lenPtr = new IntPtr(len);
            UIntPtr uLenPtr = new UIntPtr((uint)len);

            IntPtr argLoadLibrary = Exporter.VirtualAllocEx(openedProcess, IntPtr.Zero, lenPtr, AllocationType.Reserve | AllocationType.Commit, MemoryProtection.ReadWrite);

            IntPtr writedBytesCount;

            Boolean writed = Exporter.WriteProcessMemory(openedProcess, argLoadLibrary, System.Text.Encoding.ASCII.GetBytes(dllPath), uLenPtr, out writedBytesCount);

            IntPtr threadIdOut;
            IntPtr threadId = Exporter.CreateRemoteThread(openedProcess, IntPtr.Zero, 0, loadLibraryAddr, argLoadLibrary, 0, out threadIdOut);

            Exporter.CloseHandle(threadId);
        }
    }
}
