using System;
using System.Diagnostics;
using System.Linq;

namespace ThunderHawk.StaticClasses.Soulstorm
{
    public static class ProcessManager
    {
        private static Process[] GetGameProcesses()
        {
            var current = Process.GetCurrentProcess();
            return Process.GetProcessesByName("Soulstorm").Where(x => x.Id != current.Id).ToArray();
        }

        public static bool GameIsRunning()
        {
            return GetGameProcesses().Length > 0;
        }

        public static Process GetGameProcess()
        {
            return GetGameProcesses().FirstOrDefault();
        }

        public static void KillAllGameProccessesWithoutWindow()
        {
            foreach (var proc in GetGameProcesses().Where(x => x.MainWindowHandle != IntPtr.Zero))
                proc.Kill();
        }
    }
}
