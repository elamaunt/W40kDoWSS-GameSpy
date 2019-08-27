using GSMasterServer.Data;
using Steamworks;
using ThunderHawk.Core;
using ThunderHawk.Utils;
using GameServer = Steamworks.GameServer;

namespace ThunderHawk
{
    public class SteamApiService : ISteamApiService
    {
        public string NickName { get; }
        public SteamApiService()
        {
            if (SteamAPI.RestartAppIfNecessary(new AppId_t(9450)))
            {
                Logger.Error("App Restart Requested!");
            }

            if (!SteamAPI.Init())
            {
                Logger.Error("Cant init SteamApi");
            }

            var appId = SteamUtils.GetAppID();
            if (appId.m_AppId != 9450)
            {
                Logger.Error("Wrong App Id!");
            }
            NickName = SteamFriends.GetPersonaName();

            CoreContext.ServerListRetrieve.StartReloadingTimer();

            GameServer.RunCallbacks();
            SteamAPI.RunCallbacks();
            PortBindingManager.UpdateFrame();
        }
    }
}
