using Steamworks;
using System;
using System.Threading;
using System.Threading.Tasks;
using ThunderHawk.Core;
using ThunderHawk.Utils;

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

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    GameServer.RunCallbacks();
                    SteamAPI.RunCallbacks();
                    PortBindingManager.UpdateFrame();
                    Thread.Sleep(1);
                }
            }, TaskCreationOptions.LongRunning);
        }

        public string GetUserName(ulong steamId)
        {
            var id = new CSteamID(steamId);
            SteamFriends.RequestFriendRichPresence(id);
            SteamFriends.RequestUserInformation(id, true);
            return SteamFriends.GetFriendPersonaName(id);
        }

        public ulong GetUserSteamId()
        {
            return SteamUser.GetSteamID().m_SteamID;
        }

        public DateTime GetCurrentTime()
        {
            return new DateTime(1970, 1, 1).AddSeconds(SteamUtils.GetServerRealTime()).ToLocalTime();
        }

        public Task<GameHostInfo[]> LoadLobbies()
        {
            return SteamLobbyManager.LoadLobbies(null, CoreContext.ClientServer.GetIndicator()).ContinueWith(task => 
            {
                var lobbies = task.Result;

                var hosts = new GameHostInfo[lobbies.Length];

                for (int i = 0; i < hosts.Length; i++)
                {
                    var lobby = lobbies[i];

                    var host = new GameHostInfo();

                    host.IsUser = lobby.HostSteamId == SteamUser.GetSteamID();
                    host.MaxPlayers = lobby.MaxPlayers.ParseToIntOrDefault();
                    host.Players = lobby.PlayersCount.ParseToIntOrDefault();
                    host.Ranked = lobby.Ranked;
                    host.GameVariant = lobby.GameVariant;
                    host.Teamplay = lobby.IsTeamplay;

                    hosts[i] = host;
                }

                return hosts;
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}
