using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace ThunderHawk.RemoteLaunch
{
    public static class RemoteLaunch
    {
        public const string LauncherInstalledPathFileName = "LauncherPath.p";

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

                if (!File.Exists(LauncherInstalledPathFileName))
                {
                    Launch();
                    return;
                }

                var launcherPath = File.ReadAllText(LauncherInstalledPathFileName);
                var launcherExePath = Path.Combine(launcherPath, "ThunderHawk.exe");

                if (File.Exists(launcherExePath))
                {
                    Environment.CurrentDirectory = launcherPath;
                    Directory.SetCurrentDirectory(launcherPath);
                }

                Launch();
            }
            catch (Exception ex)
            {
                File.WriteAllText("StartupException.ex", ex.GetLowestBaseException().ToString());
                throw;
            }
        }

        private static void Launch()
        {
            AppDomain.CurrentDomain.ExecuteAssembly("ThunderHawk.exe");
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            File.WriteAllText("LastException.ex", (e.ExceptionObject as Exception).GetLowestBaseException().ToString());
        }

        public static Exception GetLowestBaseException(this Exception exception)
        {
            Exception baseEx;
            while (exception != (baseEx = exception.GetBaseException()) || baseEx is AggregateException)
            {
                if (baseEx is AggregateException)
                {
                    exception = ((AggregateException)baseEx).InnerException;
                    continue;
                }

                exception = baseEx;
            }
            return baseEx;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostThreadMessage(uint threadId, uint msg, IntPtr wParam, IntPtr lParam);

        public delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern bool EnumThreadWindows(uint dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    }
}
