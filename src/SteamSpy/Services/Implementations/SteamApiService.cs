using Steamworks;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ThunderHawk.Core;
using ThunderHawk.Utils;

namespace ThunderHawk
{
    public class SteamApiService : ISteamApiService
    {
        public string NickName => IsInitialized ? SteamFriends.GetPersonaName() : string.Empty;

        public bool IsInitialized { get; private set; }

        AppId_t AppId = AppId_t.Invalid;

        ulong _steamIdUnderTest;
        private CancellationTokenSource _testTokenSource;
        TaskCompletionSource<bool> _testAwaitingTask;

        readonly Callback<PersonaStateChange_t> _personaStateChangeCallback;
        readonly Callback<FriendRichPresenceUpdate_t> _richPresenceUpdateCallback;

        public event Action<ulong> UserStateChanged;
        public event Action<ulong> UserRichPresenceChanged;

        public ulong SteamId => SteamUser.GetSteamID().m_SteamID;

        public SteamApiService()
        {
            _personaStateChangeCallback = Callback<PersonaStateChange_t>.Create(OnPersonaStateChange);
            _richPresenceUpdateCallback = Callback<FriendRichPresenceUpdate_t>.Create(OnFriendRichPresenceChanged);

            PortBindingManager.TestBufferReceived += OnTestBufferReceived;
        }

        private void OnTestBufferReceived(CSteamID remoteId, byte[] buffer, uint bytesCount)
        {
            if (buffer[0] == 1)
            {
                if (_steamIdUnderTest == remoteId.m_SteamID)
                    _testAwaitingTask?.TrySetResult(true);
            }
            else
            {
                SteamUserStates.SendSuccededTestBuffer(remoteId.m_SteamID, 1200, 2);
            }
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
        /*
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
        }*/

        public void TestConnectionWithPlayer(ulong steamId)
        {
            if (_testAwaitingTask != null)
            {
                MessageBox.Show("Test already in progress. Please, wait for result");
                return;
            }

            _testTokenSource = new CancellationTokenSource();
            _testAwaitingTask = new TaskCompletionSource<bool>();
            _steamIdUnderTest = steamId;

            _testAwaitingTask.Task.ContinueWith(t =>
            {
                _testAwaitingTask = null;
                _testTokenSource?.Dispose();
                _testTokenSource = null;

                if (t.IsCanceled || t.IsFaulted)
                {
                    MessageBox.Show("Test failed. Connection isn't established with the player");
                }
                else
                {
                    MessageBox.Show("Test succeded. Connection is stable");
                }
            }, TaskContinuationOptions.AttachedToParent);

            _testTokenSource.Token.Register(() => _testAwaitingTask.TrySetCanceled());
            SteamUserStates.SendTestBufferAndCheckConnection(steamId, 1200, 2);
            _testTokenSource.CancelAfter(10000);
        }

        public void ResetPortBindingWithPlayer(ulong steamId)
        {
            PortBindingManager.AddOrUpdatePortBinding(new CSteamID(steamId)).ResetLocalPoint();
        }
    }
}
