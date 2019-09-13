using Framework;
using Framework.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using ThunderHawk.Core;
using ThunderHawk.StaticClasses.Soulstorm;
using Module = Framework.Module;

namespace ThunderHawk
{
    public partial class App
    {
        public App()
        {
            InitializeComponent();
        }

        protected override IEnumerable<Module> CreateModules()
        {
            yield return new FrameworkModule();
            yield return new FrameworkWPFModule();
            yield return new ThunderHawkCoreModule();
            yield return new ApplicationModule();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (PathFinder.GamePath != null)
            {
                var steamworksDllPathInGameFolder = Path.Combine(PathFinder.GamePath, "Steamworks.NET.dll");
                if (File.Exists(steamworksDllPathInGameFolder))
                    File.Delete(steamworksDllPathInGameFolder);

                var steamworksPdbPathInGameFolder = Path.Combine(PathFinder.GamePath, "Steamworks.NET.pdb");
                if (File.Exists(steamworksPdbPathInGameFolder))
                    File.Delete(steamworksPdbPathInGameFolder);
            }

            if (Environment.Is64BitProcess)
                File.Copy(Path.Combine(Environment.CurrentDirectory, "steam_api64.dll"), Path.Combine(Environment.CurrentDirectory, "steam_api_th.dll"), true);
            else
                File.Copy(Path.Combine(Environment.CurrentDirectory, "steam_api86.dll"), Path.Combine(Environment.CurrentDirectory, "steam_api_th.dll"), true);

            base.OnStartup(e);

#if SPACEWAR
            MainWindow = new MainWindow();
#else
            CoreContext.SteamApi.Initialize();
            CoreContext.UpdaterService.CheckForUpdates();

            var window = WPFPageHelper.InstantiateWindow<MainWindowViewModel>();
            window.Show();
            MainWindow = window;
#endif

            var path = Path.Combine(CoreContext.LaunchService.LauncherPath, "logs");

            NLog.LogManager.Configuration = LogConfigurator.GetConfiguration(path);
        }
    }
}
