﻿using Microsoft.Win32;
using Steamworks;
using System;
using System.IO;
using System.Windows;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class SteamApiService : ISteamApiService
    {
        public string NickName => IsInitialized ? SteamFriends.GetPersonaName() : string.Empty;

        public bool IsInitialized { get; private set; }

#if SPACEWAR
        AppId_t AppId = AppId_t.Invalid;
#else
        AppId_t AppId = new AppId_t(9450);
#endif

        public void Initialize()
        {
            var gamePath = CoreContext.LaunchService.GamePath;

            if (gamePath == null)
            {
                MessageBox.Show("Soulstorm steam game not found");
                return;
            }

            if (SteamAPI.RestartAppIfNecessary(AppId))
            {
                RestartAsSoulstormExe();
                return;
            }

            if (!SteamAPI.Init())
                throw new Exception("Cant init SteamApi");

            var appId = SteamUtils.GetAppID();

            if (appId.m_AppId != AppId.m_AppId)
                throw new Exception("Wrong App Id!");

            IsInitialized = true;
        }

        private void RestartAsSoulstormExe()
        {
            var gamePath = CoreContext.LaunchService.GamePath;

            //File.WriteAllText(Path.Combine(gamePath, ThunderHawk.PathContainerName), Environment.CurrentDirectory);
            var regKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).CreateSubKey(ThunderHawk.RegistryKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
            regKey.SetValue("Path", Environment.CurrentDirectory);

            File.Copy(Path.Combine(Environment.CurrentDirectory, "ThunderHawk.RemoteLaunch.exe"), Path.Combine(gamePath, "Soulstorm.exe"), true);
            File.Copy(Path.Combine(Environment.CurrentDirectory, "ThunderHawk.exe.config"), Path.Combine(gamePath, "Soulstorm.exe.config"), true);

            Environment.Exit(0);
        }
    }
}
