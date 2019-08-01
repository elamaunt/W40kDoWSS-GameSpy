using Steamworks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameServer = GSMasterServer.Data.GameServer;

namespace SteamSpy.Utils
{
    public static class SteamLobbyManager
    {
        static readonly object LOCK = new object();
        static CSteamID? _currentLobby;
        static GameServer _currentServer;

        public static bool IsLobbyJoinable => IsInLobbyNow && _currentServer.Valid;

        public static bool IsInLobbyNow => _currentLobby != null;

        public static void LeaveFromCurrentLobby()
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

        public static Task<CSteamID> CreatePublicLobby(GameServer server, CancellationToken token)
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
                          _currentServer = server;

                          UpdateCurrentLobby(server);

                          Console.WriteLine("Лобби успешно создано. ID: " + id.m_SteamID);
                          tcs.TrySetResult(id);
                      });
            }
        }

        public static void UpdateCurrentLobby(GameServer server)
        {
            lock (LOCK)
            {
                if (_currentLobby == null)
                    return;

                var id = _currentLobby.Value;

                SteamMatchmaking.SetLobbyJoinable(id, server.Valid);
                SteamMatchmaking.SetLobbyData(id, LobbyDataKeys.STEAM_SPY_INDICATOR, SteamConstants.INDICATOR);

                var hostId = SteamUser.GetSteamID().m_SteamID.ToString();
                SteamMatchmaking.SetLobbyData(id, LobbyDataKeys.HOST_STEAM_ID, hostId);

                Console.WriteLine("Задан HOST ID "+ hostId);

                _currentServer.Valid = server.Valid;

                foreach (var item in server.Properties)
                {
                    SteamMatchmaking.SetLobbyData(id, item.Key, item.Value);

                    if (_currentServer != server)
                        _currentServer.Set(item.Key, item.Value);
                }
            }
        }

        public static Task<GameServer[]> LoadLobbies()
        {
            lock (LOCK)
            {
                SteamApiHelper.CancelApiCall<LobbyMatchList_t>();

                SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
                SteamMatchmaking.AddRequestLobbyListStringFilter(LobbyDataKeys.STEAM_SPY_INDICATOR, SteamConstants.INDICATOR, ELobbyComparison.k_ELobbyComparisonEqual);
                SteamMatchmaking.AddRequestLobbyListStringFilter(LobbyDataKeys.GAME_VARIANT, SteamConstants.GameVariant, ELobbyComparison.k_ELobbyComparisonEqual);
                /* SteamMatchmaking.AddRequestLobbyListStringFilter(LobbyDataKeys.LOBBY_TYPE, SteamConstants.LOBBY_TYPE_DEFAULT, ELobbyComparison.k_ELobbyComparisonEqual);
                 SteamMatchmaking.AddRequestLobbyListStringFilter(LobbyDataKeys.GAME_VERSION, GameConstants.VERSION, ELobbyComparison.k_ELobbyComparisonEqual);
                 SteamMatchmaking.AddRequestLobbyListStringFilter(LobbyDataKeys.IS_IN_GAME, false.ToString(), ELobbyComparison.k_ELobbyComparisonEqual);
                 SteamMatchmaking.AddRequestLobbyListStringFilter(LobbyDataKeys.IS_AUTOMATCH, false.ToString(), ELobbyComparison.k_ELobbyComparisonEqual);
                 SteamMatchmaking.AddRequestLobbyListStringFilter(LobbyDataKeys.IS_DUEL, false.ToString(), ELobbyComparison.k_ELobbyComparisonEqual);*/

                return SteamApiHelper.HandleApiCall<GameServer[], LobbyMatchList_t>(SteamMatchmaking.RequestLobbyList(), CancellationToken.None,
                    (tcs, result, bIOFailure) =>
                    {
                        if (bIOFailure)
                            return;

                        Console.WriteLine("Лобби найдено " + result.m_nLobbiesMatching);

                        tcs.SetResult(HandleGameLobbies(result));
                    });
            }
        }

        private static GameServer[] HandleGameLobbies(LobbyMatchList_t param)
        {
            var lobbies = new List<GameServer>();

            for (int i = 0; i < param.m_nLobbiesMatching; i++)
            {
                var lobbyId = SteamMatchmaking.GetLobbyByIndex(i);

                if (SteamMatchmaking.RequestLobbyData(lobbyId))
                {
                    var indicator = SteamMatchmaking.GetLobbyData(lobbyId, LobbyDataKeys.STEAM_SPY_INDICATOR);

                    if (!indicator.IsNullOrEmpty() && indicator == SteamConstants.INDICATOR)
                    {
                        var ownerId = SteamMatchmaking.GetLobbyData(lobbyId, LobbyDataKeys.HOST_STEAM_ID);
                        var id = ownerId.ParseToUlongOrDefault();

                        if (id == 0)
                            continue;

                        var ownerSteamId = new CSteamID(id);

                        if (ownerSteamId == SteamUser.GetSteamID())
                            continue;
                       
                        var server = new GameServer();

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
            public const string STEAM_SPY_INDICATOR = "gameVersion";
            public const string HOST_STEAM_ID = "hostSteamId";
            public const string GAME_VARIANT = "gamevariant";
        }
        
    }
}
