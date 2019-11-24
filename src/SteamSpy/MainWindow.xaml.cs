using Steamworks;
using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Media;
using ThunderHawk.Core;
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

            var process = Process.Start("ThunderHawk.HostsFixer.exe", "127.0.0.1");
            process.WaitForExit();

            CompositionTarget.Rendering += OnRender;
            CoreContext.ClientServer.Start();
            CoreContext.MasterServer.Connect(SteamUser.GetSteamID().m_SteamID);
        }

        private void OnRender(object sender, EventArgs e)
        {
            GameServer.RunCallbacks();
            SteamAPI.RunCallbacks();
            PortBindingManager.UpdateFrame();
        }
    }
}
