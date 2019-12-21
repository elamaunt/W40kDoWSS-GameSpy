using Steamworks;
using System;
using System.Collections.Concurrent;
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

        static int? _maxPlayers;

        static readonly byte[] _chatMessageBuffer = new byte[4096];

        public static bool IsLobbyJoinable => IsInLobbyNow && _currentServer != null && _currentServer.Valid;

        public static bool IsInLobbyNow => _currentLobby != null;

        public static bool IsLobbyFull
        {
            get
            {
                if (!IsInLobbyNow)
                    return false;

                var limit = SteamMatchmaking.GetLobbyMemberLimit(_currentLobby.Value);

                if (limit == 0)
                    return false;

                return SteamMatchmaking.GetNumLobbyMembers(_currentLobby.Value) == limit;
            }
        }

        public static CSteamID? CurrentLobbyId => _currentLobby;

        static readonly Callback<LobbyChatUpdate_t> _lobbyChatUpdateCallback;
        static readonly Callback<LobbyDataUpdate_t> _lobbyDataUpdateCallback;
        static readonly Callback<LobbyChatMsg_t> _lobbyChatMessageCallback;

        public static event Action<ulong, string> LobbyMemberUpdated;
        public static event Action<ulong, bool> LobbyMemberLeft;
        public static event Action<ulong, string> LobbyChatMessage;
        public static event Action<string> TopicUpdated;

        readonly static ConcurrentDictionary<ulong, string> _membersNames = new ConcurrentDictionary<ulong, string>();
        readonly static ConcurrentDictionary<string, string> _currentUserLobbyKeyValues = new ConcurrentDictionary<string, string>();
        readonly static ConcurrentQueue<string> _sendInChatQueue = new ConcurrentQueue<string>();

        static SteamLobbyManager()
        {
            _lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
            _lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
            _lobbyChatMessageCallback = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMessage);
        }


        public static void SaveMemberName(ulong steamId, string name)
        {
            _membersNames[steamId] = name;
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

        public static string GetLobbyTopic()
        {
            if (!_currentLobby.HasValue)
                return null;

            return SteamMatchmaking.GetLobbyData(_currentLobby.Value, LobbyDataKeys.TOPIC);
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
               /* case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
                    { 
                        var name = SteamMatchmaking.GetLobbyMemberData(_currentLobby.Value, new CSteamID(update.m_ulSteamIDUserChanged), LobbyDataKeys.MEMBER_NAME);
                        var profileId = SteamMatchmaking.GetLobbyMemberData(_currentLobby.Value, new CSteamID(update.m_ulSteamIDUserChanged), LobbyDataKeys.MEMBER_PROFILE_ID);

                        LobbyMemberEntered(update.m_ulSteamIDUserChanged, profileId, name);
                        break;
                    }*/
                case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
                case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
                case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
                case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
                    {
                        LobbyMemberLeft(update.m_ulSteamIDUserChanged, change != EChatMemberStateChange.k_EChatMemberStateChangeLeft);
                        break;
                    }
                default:
                    break;
            }
        }

        public static string GetKeyValue(string name, string key)
        {
            if (_currentLobby == null)
                return string.Empty;

            var count = SteamMatchmaking.GetNumLobbyMembers(_currentLobby.Value);

            for (int i = 0; i < count; i++)
            {
                var id = SteamMatchmaking.GetLobbyMemberByIndex(_currentLobby.Value, i);
                var memberName = SteamMatchmaking.GetLobbyMemberData(_currentLobby.Value, id, LobbyDataKeys.MEMBER_NAME);
                if (memberName == name)
                    return SteamMatchmaking.GetLobbyMemberData(_currentLobby.Value, id, key);
            }

            return string.Empty;
        }

        public static int GetLobbyMembersCount()
        {
            if (_currentLobby == null)
                return 0;

            return SteamMatchmaking.GetNumLobbyMembers(_currentLobby.Value);
        }

        public static string GetLobbyMemberName(int i)
        {
            return GetLobbyMemberData(i, LobbyDataKeys.MEMBER_NAME);
        }

        public static string GetLobbyMemberData(ulong id, string key)
        {
            if (_currentLobby == null)
                return null;

            return SteamMatchmaking.GetLobbyMemberData(_currentLobby.Value, new CSteamID(id), key);
        }

        public static string GetLobbyMemberData(int i, string key)
        {
            if (_currentLobby == null)
                return null;

            var id = SteamMatchmaking.GetLobbyMemberByIndex(_currentLobby.Value, i);

            if (id == SteamUser.GetSteamID())
                if (_currentUserLobbyKeyValues.TryGetValue(key, out string value))
                    return value;

            return SteamMatchmaking.GetLobbyMemberData(_currentLobby.Value, id, key);
        }

        public static void SetKeyValue(string key, string value)
        {
            lock (LOCK)
            {
                _currentUserLobbyKeyValues[key] = value;

                if (_currentLobby == null)
                    return;

                SteamMatchmaking.SetLobbyMemberData(SteamUser.GetSteamID(), key, value);
            }
        }

        public static string GetLocalMemberData(string key)
        {
            if (_currentUserLobbyKeyValues.TryGetValue(key, out string value))
                return value;

            return string.Empty;
        }

        public static int GetLobbyMaxPlayers()
        {
            if (_currentLobby == null)
                return -1;

            return SteamMatchmaking.GetLobbyMemberLimit(_currentLobby.Value);
        }

        public static void SetLobbyMaxPlayers(int max)
        {
            if (_currentLobby == null)
            {
                _maxPlayers = max;
                return;
            }

            SteamMatchmaking.SetLobbyMemberLimit(_currentLobby.Value, max);
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
            lock (LOCK)
            {
                if (_currentLobby == null)
                {
                    _sendInChatQueue.Enqueue(message);
                    return;
                }

                Logger.Info("SEND-TO-LOBBY-CHAT "+ message);

                var bytes = Encoding.UTF8.GetBytes(message);

                SteamMatchmaking.SendLobbyChatMsg(_currentLobby.Value, bytes, bytes.Length);
            }
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

                        //SteamMatchmaking.SetLobbyJoinable(id, false);
                        SteamMatchmaking.SetLobbyData(id, LobbyDataKeys.HOST_STEAM_ID, "0");

                        SteamMatchmaking.LeaveLobby(id);
                        Console.WriteLine("Лобби покинуто " + id);
                        _currentLobby = null;
                        _currentServer = null;
                        _currentUserLobbyKeyValues.Clear();
                        _sendInChatQueue.Clear();
                        _maxPlayers = null;
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

        public static Task<bool> EnterInLobby(CSteamID lobbyId, string shortUser, string name, string profileId, CancellationToken token)
        {
            LeaveFromCurrentLobby();
            Console.WriteLine("Вход в лобби Steam");
            return SteamApiHelper.HandleApiCall<bool, LobbyEnter_t>(SteamMatchmaking.JoinLobby(lobbyId), token,
                      (tcs, result, bIOFailure) =>
                      {
                          var resp = (EChatRoomEnterResponse)result.m_EChatRoomEnterResponse;

                          lock(LOCK)
                          {
                              var id = new CSteamID(result.m_ulSteamIDLobby);
                              _currentLobby = id;

                              if (resp == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                              {
                                  SteamMatchmaking.SetLobbyMemberData(id, LobbyDataKeys.USER, shortUser);
                                  SteamMatchmaking.SetLobbyMemberData(id, LobbyDataKeys.MEMBER_NAME, name);
                                  SteamMatchmaking.SetLobbyMemberData(id, LobbyDataKeys.MEMBER_PROFILE_ID, profileId);

                                  foreach (var pair in _currentUserLobbyKeyValues)
                                      SteamMatchmaking.SetLobbyMemberData(id, pair.Key, pair.Value);

                                  while (_sendInChatQueue.TryDequeue(out string mes))
                                      SendInLobbyChat(mes);
                              }
                          }

                          tcs.SetResult(resp == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess);
                          Console.WriteLine("Вход в лобби Steam завершен");
                      });
        }

        public static Task<CSteamID> CreatePublicLobby(CancellationToken token, string name, string shortUser, string flags, string indicator)
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

                          SteamMatchmaking.SetLobbyMemberData(id, LobbyDataKeys.USER, shortUser);
                          SteamMatchmaking.SetLobbyMemberData(id, LobbyDataKeys.MEMBER_NAME, name);
                          SteamMatchmaking.SetLobbyData(id, LobbyDataKeys.THUNDERHAWK_INDICATOR, indicator);

                          foreach (var pair in _currentUserLobbyKeyValues)
                              SteamMatchmaking.SetLobbyMemberData(id, pair.Key, pair.Value);

                          if (_maxPlayers.HasValue)
                              SteamMatchmaking.SetLobbyMemberLimit(id, _maxPlayers.Value);

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

        public static void SetLobbyJoinable(bool joinable)
        {
            lock (LOCK)
            {
                if (_currentLobby == null)
                    return;

                SteamMatchmaking.SetLobbyJoinable(_currentLobby.Value, joinable);
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

                        if (server.Ranked)
                            server.Set("numplayers", SteamMatchmaking.GetNumLobbyMembers(lobbyId).ToString());

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
            public const string MEMBER_PROFILE_ID = "memberProfileId";
            public const string USER = "username";
            public const string MEMBER_NAME = "memberName";
            public const string THUNDERHAWK_INDICATOR = "gameVersion";
            public const string HOST_STEAM_ID = "hostSteamId";
            public const string GAME_VARIANT = "gamevariant";
        }
    }
}
