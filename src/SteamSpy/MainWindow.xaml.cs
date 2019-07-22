﻿using SteamSpy.Utils;
using Steamworks;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace SteamSpy
{
    public partial class MainWindow : Window
    {
        Process _soulstormProcess;

        public MainWindow()
        {
            InitializeComponent();
            
            CompositionTarget.Rendering += OnRender;
           // if (SteamAPI.RestartAppIfNecessary(new AppId_t(685420))) 
            if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
            {
                Console.WriteLine("APP RESTART REQUESTED");
                Environment.Exit(0);
            }

            if (SteamAPI.Init())
                Console.WriteLine("Steam inited");
            else
                throw new Exception("Не удалось запустить игру. Клиент Steam не запущен");

           // _soulstormProcess = Process.Start(@"F:\Games\Soulstorm 1.2\Soulstorm.exe");
        }

        private void OnRender(object sender, EventArgs e)
        {
            GameServer.RunCallbacks();
            SteamAPI.RunCallbacks();
            PortBindingManager.UpdateFrame();
        }
    }
}
