using Steamworks;
using System;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class SteamApiService : ISteamApiService
    {
        public string NickName => IsInitialized ? SteamFriends.GetPersonaName() : string.Empty;

        public bool IsInitialized { get; private set; }

        AppId_t AppId = AppId_t.Invalid;

        public void Initialize()
        {
            if (!SteamAPI.Init())
                throw new Exception("Cant init SteamApi");

            IsInitialized = true;

            CoreContext.MasterServer.Connect(SteamUser.GetSteamID().m_SteamID);
        }

        public string GetUserName(ulong steamId)
        {
            return SteamFriends.GetFriendPersonaName(new CSteamID(steamId));
        }
    }
}
