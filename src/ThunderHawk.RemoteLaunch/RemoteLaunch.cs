using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Policy;

namespace ThunderHawk.RemoteLaunch
{
    public static class RemoteLaunch
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                //AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
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
                File.WriteAllText("StartupException.ex", ex/*.GetLowestBaseException()*/.ToString());
                throw;
            }
        }

        private static void Launch()
        {
            /*try
            {
                //ThunderHawk.Main();
                //var assembly = Assembly.LoadFile(Path.Combine(Environment.CurrentDirectory, "ThunderHawk.exe"));

                //assembly.EntryPoint.Invoke(null, new object[0]);
            }
            catch(Exception ex)
            {
                Debugger.Launch();
            }

          // var assembly = Assembly.LoadFile(Path.Combine(Environment.CurrentDirectory, "ThunderHawk.exe"));*/
            AppDomain.CurrentDomain.SetPrincipalPolicy(System.Security.Principal.PrincipalPolicy.WindowsPrincipal);

            EvidenceBase[] hostEvidence = { new Zone(SecurityZone.MyComputer) };
            Evidence e = new Evidence(hostEvidence, null);

            AppDomain.CurrentDomain.ExecuteAssembly("ThunderHawk.exe", e);
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {

            Assembly executingAssembly = Assembly.GetExecutingAssembly();

            string applicationDirectory = Environment.CurrentDirectory; //Path.GetDirectoryName(executingAssembly.Location);

            string[] fields = args.Name.Split(',');
            string assemblyName = fields[0];
            string assemblyCulture;
            if (fields.Length < 2)
                assemblyCulture = null;
            else
                assemblyCulture = fields[2].Substring(fields[2].IndexOf('=') + 1);

            if (assemblyName.EndsWith(".resources") && !assemblyCulture.EndsWith("neutral"))
                return null;

            string assemblyFileName = assemblyName + ".dll";
            string exeFileName = assemblyName + ".exe";

            string assemblyPath;
            string exePath;

            if (assemblyName.EndsWith(".resources"))
            {
                // Specific resources are located in app subdirectories
                string resourceDirectory = Path.Combine(Path.GetDirectoryName(executingAssembly.Location), assemblyCulture);
               // string resourceDirectory = Path.Combine(applicationDirectory, assemblyCulture);
               
                assemblyPath = Path.Combine(resourceDirectory, assemblyFileName);
                exePath = Path.Combine(resourceDirectory, exeFileName);
            }
            else
            {
                assemblyPath = Path.Combine(applicationDirectory, assemblyFileName);
                exePath = Path.Combine(applicationDirectory, exeFileName);
            }

            try
            {
                if (File.Exists(exePath))
                    return Assembly.LoadFrom(exePath);

                return Assembly.LoadFrom(assemblyPath);
            }
            catch (Exception ex)
            {
                File.WriteAllText("AssemblyLoadException.ex", assemblyPath + " => " + ex.GetLowestBaseException().ToString());
                throw;
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            File.WriteAllText("LastException.ex", (e.ExceptionObject as Exception)/*.GetLowestBaseException()*/.ToString());
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
