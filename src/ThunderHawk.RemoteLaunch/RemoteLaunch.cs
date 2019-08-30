using System;
using System.IO;

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
    }
}
