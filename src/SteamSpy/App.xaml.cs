//using DesktopNotifications;
using Framework;
using Framework.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
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
                File.Copy(Path.Combine(Environment.CurrentDirectory, "steam_api64.dll"),
                    Path.Combine(Environment.CurrentDirectory, "steam_api_th.dll"), true);
            else
                File.Copy(Path.Combine(Environment.CurrentDirectory, "steam_api86.dll"),
                    Path.Combine(Environment.CurrentDirectory, "steam_api_th.dll"), true);

            base.OnStartup(e);

            if (!CoreContext.SystemService.IsSteamRunning)
            {
                String steamPath;

                try
                {
                    steamPath = CoreContext.SystemService.GetSteamExePath();
                }
                catch (Exception ex)
                {
                    Logger.Error("Can't find steam path: " + ex);
                    
                    // TODO перенести в отдельный класс для алертов
                    Form f = new Form();
                    f.Icon = new Icon("thunderhaw_notify.ico");
                    f.Size = new System.Drawing.Size(400, 150);
                    f.Text = "Can't detect steam";
                    Label label = new Label();
                    label.Location = new System.Drawing.Point(15, 20);
                    label.Size = new System.Drawing.Size(300, 30);
                    label.Text = "You should install steam to play on this server.";
                    f.Controls.Add(label);
                    
                    Button goToSteamWebSite = new Button();
                    // Configure the LinkLabel's location. 
                    goToSteamWebSite.SetBounds(15,70, 350,20);
                    goToSteamWebSite.Click += (sender, args) =>
                    {
                        Process.Start("https://store.steampowered.com/about/");
                    };

                    // Set the text for the LinkLabel.
                    goToSteamWebSite.Text = "Download steam";
                    f.Controls.Add(goToSteamWebSite);
                    
                    f.Show();
                    f.FormClosed += (sender, args) =>
                    {
                        Environment. Exit(0);
                    };
                    return;
                }


                if (steamPath != null)
                {
                    Process.Start(steamPath, "-silent").WaitForInputIdle();

                    Thread.Sleep(10000);
                }
            }

            try
            {
                CoreContext.SteamApi.Initialize();
            }
            catch(Exception ex)
            {
                Logger.Error("Can't init steam api: " + ex);
                    
                // TODO перенести в отдельный класс для алертов
                Form f = new Form();
                f.Icon = new Icon("thunderhaw_notify.ico");
                f.Size = new System.Drawing.Size(400, 150);
                f.Text = "Can't launch steam api";
                Label label = new Label();
                label.Location = new System.Drawing.Point(15, 20);
                label.Size = new System.Drawing.Size(300, 30);
                label.Text = "LogIn to steam and try again";
                f.Controls.Add(label);
                    
                f.FormClosed += (sender, args) =>
                {
                    Environment. Exit(0);
                };
                return;
            }
            
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