using Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ThunderHawk.StaticClasses.Soulstorm;

namespace ThunderHawk
{
    public static class ThunderHawk
    {
        public const string RegistryKey = "SoftWare\\ThunderHawk";

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                PathFinder.Find();

                if (!args.IsNullOrEmpty() && args[0] == "-original")
                {
                    Thread.Sleep(2000);
                    File.Copy(Path.Combine(Environment.CurrentDirectory, "GameFiles", "SteamSSBackup", "Soulstorm.exe" ), Path.Combine(PathFinder.GamePath, "Soulstorm.exe"), true);
                    Thread.Sleep(2000);
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
