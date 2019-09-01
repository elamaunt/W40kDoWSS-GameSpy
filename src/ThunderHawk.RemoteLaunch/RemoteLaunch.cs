using Microsoft.Win32;
using System;
using System.IO;

namespace ThunderHawk.RemoteLaunch
{
    public static class RemoteLaunch
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

                var regKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("Software\\ThunderHawk");
                string launcherPath = null;
                if (regKey != null)
                {
                    launcherPath = (string)regKey.GetValue("Path");
                }

                if (launcherPath == null)
                {
                    Launch();
                    return;
                }


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
    }
}
