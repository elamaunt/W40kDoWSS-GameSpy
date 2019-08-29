using Steamworks;
using System;
using System.IO;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class SteamApiService : ISteamApiService
    {
        public string NickName => IsInitialized ? SteamFriends.GetPersonaName() : string.Empty;

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
           // try
           // {
                if (SteamAPI.RestartAppIfNecessary(new AppId_t(9450)))
                {
                    RestartAsSoulstormExe();
                    return;
                }

                if (!SteamAPI.Init())
                     throw new Exception("Cant init SteamApi");

                var appId = SteamUtils.GetAppID();

                if (appId.m_AppId != 9450)
                    throw new Exception("Wrong App Id!");

                IsInitialized = true;
           //}
           // catch (Exception ex)
           // {
           //     Logger.Error(ex);
           // }
        }

        private void RestartAsSoulstormExe()
        {
            var gamePath = CoreContext.LaunchService.GamePath;

            File.WriteAllText(Path.Combine(gamePath, ThunderHawk.PathContainerName), Environment.CurrentDirectory);
            File.Copy(Path.Combine(Environment.CurrentDirectory, "ThunderHawk.RemoteLaunch.exe"), Path.Combine(gamePath, "Soulstorm.exe"), true);

            Environment.Exit(0);
        }
    }
}
