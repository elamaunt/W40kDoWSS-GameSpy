using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThunderHawk.Core;
using GameServer = GSMasterServer.Data.GameServer;

namespace ThunderHawk.Utils
{
    public static class SteamLobbyManager
    {
        static readonly object LOCK = new object();
        static CSteamID? _currentLobby;
        static GameServer _currentServer;
        static readonly byte[] _chatMessageBuffer = new byte[4096];

        public static bool IsLobbyJoinable => IsInLobbyNow && _currentServer.Valid;

        public static bool IsInLobbyNow => _currentLobby != null;

        private static readonly Callback<LobbyChatUpdate_t> _lobbyChatUpdateCallback;
        private static readonly Callback<LobbyDataUpdate_t> _lobbyDataUpdateCallback;
        private static readonly Callback<LobbyChatMsg_t> _lobbyChatMessageCallback;

        public static event Action<ulong, string> LobbyMemberUpdated;
        public static event Action<ulong, string> LobbyMemberEntered;
        public static event Action<ulong, string> LobbyMemberLeft;
        public static event Action<ulong, string> LobbyChatMessage;
        public static event Action<string> TopicUpdated;

        static SteamLobbyManager()
        {
            _lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
            _lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
            _lobbyChatMessageCallback = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMessage);
        }

        static void OnLobbyChatMessage(LobbyChatMsg_t message)
        {
            if (!_currentLobby.HasValue)
                return;

            var type = (EChatEntryType)message.m_eChatEntryType;

            if (type != EChatEntryType.k_EChatEntryTypeChatMsg)
                return;

            if (message.m_ulSteamIDLobby != _currentLobby.Value.m_SteamID)
                return;

            var length = SteamMatchmaking.GetLobbyChatEntry(_currentLobby.Value, (int)message.m_iChatID, out CSteamID userId, _chatMessageBuffer, _chatMessageBuffer.Length, out type);

            var text = Encoding.UTF8.GetString(_chatMessageBuffer, 0, length);

            LobbyChatMessage?.Invoke(message.m_ulSteamIDUser, text);
        }

        static void OnLobbyDataUpdate(LobbyDataUpdate_t update)
        {
            if (!_currentLobby.HasValue)
                return;

            if (update.m_ulSteamIDMember == _currentLobby.Value.m_SteamID)
            {
                TopicUpdated?.Invoke(SteamMatchmaking.GetLobbyData(_currentLobby.Value, LobbyDataKeys.TOPIC));
            }
            else
            {
                var name = SteamMatchmaking.GetLobbyMemberData(_currentLobby.Value, new CSteamID(update.m_ulSteamIDMember), LobbyDataKeys.MEMBER_NAME);
                LobbyMemberUpdated?.Invoke(update.m_ulSteamIDMember, name);
            }
        }

        static void OnLobbyChatUpdate(LobbyChatUpdate_t update)
        {
            if (!_currentLobby.HasValue)
                return;

            if (_currentLobby.Value.m_SteamID != update.m_ulSteamIDLobby)
                return;

            var change = (EChatMemberStateChange)update.m_rgfChatMemberStateChange;

            switch (change)
            {
                case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
                    { 
                        var name = SteamMatchmaking.GetLobbyMemberData(_currentLobby.Value, new CSteamID(update.m_ulSteamIDUserChanged), LobbyDataKeys.MEMBER_NAME);
                        LobbyMemberEntered(update.m_ulSteamIDUserChanged, name);
                        break;
                    }
                case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
                case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
                case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
                case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
                    {
                        var name = SteamMatchmaking.GetLobbyMemberData(_currentLobby.Value, new CSteamID(update.m_ulSteamIDUserChanged), LobbyDataKeys.MEMBER_NAME);
                        LobbyMemberLeft(update.m_ulSteamIDUserChanged, name);
                        break;
                    }
                default:
                    break;
            }
        }

        public static void SetLobbyTopic(string topic)
        {
            if (_currentLobby == null)
                return;

            SteamMatchmaking.SetLobbyData(_currentLobby.Value, LobbyDataKeys.TOPIC, topic);
            TopicUpdated?.Invoke(topic);
        }

        public static void SendInLobbyChat(string message)
        {
            if (_currentLobby == null)
                return;

            var bytes = Encoding.UTF8.GetBytes(message);

            SteamMatchmaking.SendLobbyChatMsg(_currentLobby.Value, bytes, bytes.Length);
        }

        public static void LeaveFromCurrentLobby()
        {
            try
            {
                lock (LOCK)
                {
                    if (_currentLobby.HasValue)
                    {
                        var id = _currentLobby.Value;

                        SteamMatchmaking.SetLobbyJoinable(id, false);
                        SteamMatchmaking.SetLobbyData(id, LobbyDataKeys.HOST_STEAM_ID, "0");

                        SteamMatchmaking.LeaveLobby(id);
                        Console.WriteLine("Лобби покинуто " + id);
                        _currentLobby = null;
                        _currentServer = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        internal static string[] GetCurrentLobbyMembers()
        {
            if (_currentLobby == null)
                return new string[0];

            var lobbyId = _currentLobby.Value;

            var membersCount = SteamMatchmaking.GetNumLobbyMembers(lobbyId);

            var usersInLobby = new string[membersCount];

            for (int i = 0; i < membersCount; i++)
            {
                var memberId = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i);
                usersInLobby[i] = SteamMatchmaking.GetLobbyMemberData(lobbyId, memberId, LobbyDataKeys.MEMBER_NAME);
            }

            return usersInLobby;
        }

        public static Task<bool> EnterInLobby(CSteamID lobbyId, string name, CancellationToken token)
        {
            LeaveFromCurrentLobby();
            return SteamApiHelper.HandleApiCall<bool, LobbyEnter_t>(SteamMatchmaking.JoinLobby(lobbyId), token,
                      (tcs, result, bIOFailure) =>
                      {
                          var resp = (EChatRoomEnterResponse)result.m_EChatRoomEnterResponse;

                          var id = new CSteamID(result.m_ulSteamIDLobby);
                          _currentLobby = id;

                          if (resp == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                              SteamMatchmaking.SetLobbyMemberData(id, LobbyDataKeys.MEMBER_NAME, name);

                          tcs.SetResult(resp == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess);
                      });
        }

        public static Task<CSteamID> CreatePublicLobby(CancellationToken token, string name, string indicator)
        {
            lock (LOCK)
            {
                LeaveFromCurrentLobby();

                Console.WriteLine("Создание лобби Steam");

                return SteamApiHelper.HandleApiCall<CSteamID, LobbyCreated_t>(SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 3), token,
                      (tcs, result, bIOFailure) =>
                      {
                          if (bIOFailure || result.m_eResult != EResult.k_EResultOK)
                          {
                              Console.WriteLine("Ошибка во время создания лобби");
                              tcs.TrySetException(new Exception(result.m_eResult.ToErrorStringMessage()));
                              return;
                          }

                          var id = new CSteamID(result.m_ulSteamIDLobby);

                          if (token.IsCancellationRequested)
                          {
                              Console.WriteLine("Создание лобби было отменено");
                              SteamMatchmaking.LeaveLobby(id);
                              tcs.TrySetCanceled();
                              return;
                          }

                          _currentLobby = id;

                          SteamMatchmaking.SetLobbyMemberData(id, LobbyDataKeys.MEMBER_NAME, name);
                          SteamMatchmaking.SetLobbyData(id, LobbyDataKeys.THUNDERHAWK_INDICATOR, indicator);
                          SteamMatchmaking.SetLobbyType(id, ELobbyType.k_ELobbyTypePublic);

                          Console.WriteLine("Лобби успешно создано. ID: " + id.m_SteamID);
                          tcs.TrySetResult(id);
                      });
            }
        }

        public static void UpdateCurrentLobby(GameServerDetails details, string indicator)
        {
            lock (LOCK)
            {
                if (_currentLobby == null)
                    return;

                var id = _currentLobby.Value;

                SteamMatchmaking.SetLobbyJoinable(id, details.IsValid);
                SteamMatchmaking.SetLobbyData(id, LobbyDataKeys.THUNDERHAWK_INDICATOR, indicator);

                var hostId = SteamUser.GetSteamID().m_SteamID.ToString();
                SteamMatchmaking.SetLobbyData(id, LobbyDataKeys.HOST_STEAM_ID, hostId);

                Console.WriteLine("Задан HOST ID "+ hostId);

                foreach (var item in details.Properties)
                    SteamMatchmaking.SetLobbyData(id, item.Key, item.Value);
            }
        }

        public static Task<GameServerDetails[]> LoadLobbies(string gameVariant = null, string indicator = null)
        {
            lock (LOCK)
            {
                SteamApiHelper.CancelApiCall<LobbyMatchList_t>();

                SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);

                if (indicator != null)
                    SteamMatchmaking.AddRequestLobbyListStringFilter(LobbyDataKeys.THUNDERHAWK_INDICATOR, indicator, ELobbyComparison.k_ELobbyComparisonEqual);

                if (gameVariant != null)
                    SteamMatchmaking.AddRequestLobbyListStringFilter(LobbyDataKeys.GAME_VARIANT, gameVariant, ELobbyComparison.k_ELobbyComparisonEqual);

                return SteamApiHelper.HandleApiCall<GameServerDetails[], LobbyMatchList_t>(SteamMatchmaking.RequestLobbyList(), CancellationToken.None,
                    (tcs, result, bIOFailure) =>
                    {
                        if (bIOFailure)
                            return;

                        Console.WriteLine("Лобби найдено " + result.m_nLobbiesMatching);

                        tcs.SetResult(HandleGameLobbies(result));
                    });
            }
        }

        private static GameServerDetails[] HandleGameLobbies(LobbyMatchList_t param, string indicatorFilter = null)
        {
            var lobbies = new List<GameServerDetails>();

            for (int i = 0; i < param.m_nLobbiesMatching; i++)
            {
                var lobbyId = SteamMatchmaking.GetLobbyByIndex(i);

                if (SteamMatchmaking.RequestLobbyData(lobbyId))
                {
                    var indicator = SteamMatchmaking.GetLobbyData(lobbyId, LobbyDataKeys.THUNDERHAWK_INDICATOR);

                    if (!indicator.IsNullOrEmpty() && (indicatorFilter == null || indicator == indicatorFilter))
                    {
                        var ownerId = SteamMatchmaking.GetLobbyData(lobbyId, LobbyDataKeys.HOST_STEAM_ID);
                        var id = ownerId.ParseToUlongOrDefault();

                        if (id == 0)
                            continue;

                        var ownerSteamId = new CSteamID(id);

                        if (ownerSteamId == SteamUser.GetSteamID())
                            continue;

                        var server = new GameServerDetails();

                        server.LobbySteamId = lobbyId;
                        server.HostSteamId = ownerSteamId;

                        var rowCount = SteamMatchmaking.GetLobbyDataCount(lobbyId);

                        string key;
                        string value;

                        for (int k = 0; k < rowCount; k++)
                            if (SteamMatchmaking.GetLobbyDataByIndex(lobbyId, k, out key, 100, out value, 100))
                                server.Set(key, value);

                        if (!server.HasPlayers)
                            continue;
                        
                        lobbies.Add(server);
                    }
                }
            }

            return lobbies.ToArray();
        }
        
        static class LobbyDataKeys
        {
            public const string TOPIC = "topic";
            public const string MEMBER_NAME = "memberName";
            public const string THUNDERHAWK_INDICATOR = "gameVersion";
            public const string HOST_STEAM_ID = "hostSteamId";
            public const string GAME_VARIANT = "gamevariant";
        }
    }
}
