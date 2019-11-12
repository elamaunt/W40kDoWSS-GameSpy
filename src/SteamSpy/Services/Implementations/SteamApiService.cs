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

        readonly Callback<PersonaStateChange_t> _personaStateChangeCallback;
        readonly Callback<FriendRichPresenceUpdate_t> _richPresenceUpdateCallback;

        public event Action<ulong> UserStateChanged;
        public event Action<ulong> UserRichPresenceChanged;

        public SteamApiService()
        {
            _personaStateChangeCallback = Callback<PersonaStateChange_t>.Create(OnPersonaStateChange);
            _richPresenceUpdateCallback = Callback<FriendRichPresenceUpdate_t>.Create(OnFriendRichPresenceChanged);
        }

        void OnPersonaStateChange(PersonaStateChange_t param)
        {
            UserStateChanged?.Invoke(param.m_ulSteamID);
        }

        void OnFriendRichPresenceChanged(FriendRichPresenceUpdate_t param)
        {
            UserRichPresenceChanged?.Invoke(param.m_steamIDFriend.m_SteamID);
        }

        public void Initialize()
        {
            if (!SteamAPI.Init())
                throw new Exception("Cant init SteamApi");

            IsInitialized = true;

            CoreContext.MasterServer.Connect(SteamUser.GetSteamID().m_SteamID);
        }

        public string GetUserName(ulong steamId)
        {
            var id = new CSteamID(steamId);
            SteamFriends.RequestFriendRichPresence(id);
            SteamFriends.RequestUserInformation(id, true);
            return SteamFriends.GetFriendPersonaName(id);
        }
    }
}
