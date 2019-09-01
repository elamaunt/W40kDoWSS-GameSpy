using Framework;
using Microsoft.Win32;
using System;
using System.IO;

namespace ThunderHawk
{
    public static class ThunderHawk
    {
        public const string RegistryKey = "SoftWare\\ThunderHawk";

        [STAThread]
        public static void Main()
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

                NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(Path.Combine(Environment.CurrentDirectory, "NLog.config"), true);

                new App().Run();
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
