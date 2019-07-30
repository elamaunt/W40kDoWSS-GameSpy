using SteamSpy.Utils;
using Steamworks;
using System;
using System.Windows;
using System.Windows.Media;

namespace SteamSpy
{
    public partial class MainWindow : Window
    {
        //Process _soulstormProcess;

        public MainWindow()
        {
            InitializeComponent();
            
            CompositionTarget.Rendering += OnRender;


            if (SteamAPI.RestartAppIfNecessary(new AppId_t(9450))) 
            //if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
            {
                Console.WriteLine("APP RESTART REQUESTED");
                Environment.Exit(0);
            }

            if (SteamAPI.Init())
                Console.WriteLine("Steam inited");
            else
            {
                MessageBox.Show("Клиент Steam не запущен");
                return;
            }
            // _soulstormProcess = Process.Start(@"F:\Games\Soulstorm 1.2\Soulstorm.exe");

            CoreContext.ServerListRetrieve.StartReloadingTimer();
        }

        private void OnRender(object sender, EventArgs e)
        {
            GameServer.RunCallbacks();
            SteamAPI.RunCallbacks();
            PortBindingManager.UpdateFrame();
        }
    }
}
