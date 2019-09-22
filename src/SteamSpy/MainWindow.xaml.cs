using Steamworks;
using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Media;
using ThunderHawk.Utils;

namespace ThunderHawk
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
            {
                Console.WriteLine("APP RESTART REQUESTED");
                Environment.Exit(0);
            }

            if (SteamAPI.Init())
                Console.WriteLine("Steam inited");
            else
            {
                MessageBox.Show("Клиент Steam не запущен");
                Environment.Exit(0);
                return;
            }

            var process = Process.Start("ThunderHawk.HostsFixer.exe", GameConstants.SERVER_ADDRESS);
            process.WaitForExit();

            CompositionTarget.Rendering += OnRender;
            ServerContext.Start(IPAddress.Any);
            ServerContext.ServerListRetrieve.StartReloadingTimer();
            ServerContext.MasterServer.Connect(SteamUser.GetSteamID());
        }

        private void OnRender(object sender, EventArgs e)
        {
            GameServer.RunCallbacks();
            SteamAPI.RunCallbacks();
            PortBindingManager.UpdateFrame();
        }
    }
}
