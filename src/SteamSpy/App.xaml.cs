using DesktopNotifications;
using Framework;
using Framework.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using ThunderHawk.Core;
using ThunderHawk.StaticClasses.Soulstorm;
using Module = Framework.Module;

namespace ThunderHawk
{
    public partial class App
    {
        bool _silence;

        public App(bool silence)
        {
            _silence = silence;
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
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<ThunderHawkNotificationActivator>("ThunderHawk");
            DesktopNotificationManagerCompat.RegisterActivator<ThunderHawkNotificationActivator>();

            if (PathFinder.GamePath != null)
            {
                var steamworksDllPathInGameFolder = Path.Combine(PathFinder.GamePath, "Steamworks.NET.dll");
                if (File.Exists(steamworksDllPathInGameFolder))
                    File.Delete(steamworksDllPathInGameFolder);

                var steamworksPdbPathInGameFolder = Path.Combine(PathFinder.GamePath, "Steamworks.NET.pdb");
                if (File.Exists(steamworksPdbPathInGameFolder))
                    File.Delete(steamworksPdbPathInGameFolder);
            }

            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (Environment.Is64BitProcess)
                File.Copy(Path.Combine(Environment.CurrentDirectory, "steam_api64.dll"), Path.Combine(Environment.CurrentDirectory, "steam_api_th.dll"), true);
            else
                File.Copy(Path.Combine(Environment.CurrentDirectory, "steam_api86.dll"), Path.Combine(Environment.CurrentDirectory, "steam_api_th.dll"), true);

            base.OnStartup(e);

            if (!CoreContext.SystemService.IsSteamRunning)
            {
                var steamPath = CoreContext.SystemService.GetSteamExePath();

                if (steamPath != null)
                {
                    Process.Start(steamPath, "-silent").WaitForInputIdle();

                    Thread.Sleep(10000);
                }
            }

            CoreContext.SteamApi.Initialize();
            CoreContext.UpdaterService.CheckForUpdates();

            var window = WPFPageHelper.InstantiateWindow<MainWindowViewModel>();

           
            window.Show();

            MainWindow = window;
            
            if (_silence)
                window.Visibility = Visibility.Collapsed;

            NLog.LogManager.Configuration = LogConfigurator.GetConfiguration("logs");
        }
    }
}
