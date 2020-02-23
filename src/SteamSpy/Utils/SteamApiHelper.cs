using Microsoft.Win32;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ThunderHawk.Utils
{
    internal static class SteamApiHelper
    {
        private const string KEY_BASE = @"SOFTWARE\Classes\steam\Shell\Open\Command\";

        //public static bool IsProduction => SteamUtils.GetAppID() == new AppId_t(SteamConstants.PRODUCTION_APP_ID);

        internal static string GetSteamExePath()
        {
            RegistryKey localMachine = Registry.LocalMachine;
            RegistryKey fileKey = localMachine.OpenSubKey(KEY_BASE);
            object result = null;
            if (fileKey != null)
            {
                result = fileKey.GetValue(string.Empty);
                fileKey.Close();
            }

            if (result != null && result is string)
            {
                var split = ((string)result).Split('"');

                if (split.Length > 2)
                    return split[1];

                return null;
            }

            return null;
        }

        public static IPAddress GetSteamServerPublicIp()
        {
            return NetworkHelper.ToAddr(SteamGameServer.GetPublicIP());
        }

        /*internal static GameNetworkState LoadUserState(CSteamID userId)
        {
            var presence = SteamFriends.GetFriendRichPresence(userId, SteamRichPresences.STATUS);
            GameNetworkState status;
            if (Enum.TryParse(presence, out status))
                return status;

            return GameNetworkState.Idleness;
        }*/

        /*internal static void UpdateUserInfo(ChatPlayerInfo info, CSteamID userId)
        {
            var state = SteamFriends.GetFriendPersonaState(userId);

            bool inGame = false;

            FriendGameInfo_t gameInfo;
            if (SteamFriends.GetFriendGamePlayed(userId, out gameInfo))
                inGame = gameInfo.m_gameID.AppID().m_AppId == SteamUtils.GetAppID().m_AppId;

            info.AvatarData = LoadAvatarsData(userId);
            info.Nickname = SteamFriends.GetFriendPersonaName(userId);
            info.Peer = new SteamPeer(userId);
            info.Rating = SteamFriends.GetFriendRichPresence(userId, SteamRichPresences.RATING).TryParseToInt();
            info.GamesWon = SteamFriends.GetFriendRichPresence(userId, SteamRichPresences.GAMES_WON).TryParseToInt();
            info.GamesLost = SteamFriends.GetFriendRichPresence(userId, SteamRichPresences.GAMES_LOST).TryParseToInt();
            info.Winrate = SteamFriends.GetFriendRichPresence(userId, SteamRichPresences.WINRATE).TryParseToInt();
            info.Online = state != EPersonaState.k_EPersonaStateOffline;
            info.InGame = inGame;
            info.IsUser = userId == SteamUser.GetSteamID();
            info.State = LoadUserState(userId);
            //info.InternalStatus = LoadUserInternalStatus(userId);
            info.Type = LoadUserType(userId);
            info.RatingIcon = LoadUserRatingIcon(userId);

        }*/

       /* internal static ChatPlayerInfo LoadUserInfo(CSteamID userId)
        {
            var info = new ChatPlayerInfo();
            UpdateUserInfo(info, userId);
            return info;
        }

        internal static UserAvatarData LoadAvatarsData(CSteamID userId)
        {
            var data = new UserAvatarData();

            data.SmallIcon = LoadIcon(SteamFriends.GetSmallFriendAvatar(userId));
            data.MediumIcon = LoadIcon(SteamFriends.GetMediumFriendAvatar(userId));
            data.LargeIcon = LoadIcon(SteamFriends.GetLargeFriendAvatar(userId));

            return data;
        }

        private static AvatarIconData LoadIcon(int avatarId)
        {
            AvatarIconData data = new AvatarIconData();
            data.Id = avatarId;

            uint w, h;
            if (avatarId > 0 && SteamUtils.GetImageSize(avatarId, out w, out h))
            {
                var bytesCount = w * h * 4;
                var bytes = new byte[bytesCount];
                data.Width = (int)w;
                data.Height = (int)h;

                if (SteamUtils.GetImageRGBA(avatarId, bytes, (int)bytesCount))
                {
                    data.Bytes = bytes;
                }
            }

            return data;
        }*/

       /* internal static ChatPlayerType LoadUserType(CSteamID userId)
        {
            return ChatPlayerType.Simple;
        }

        internal static RatingIconType LoadUserRatingIcon(CSteamID userId)
        {
            return (RatingIconType)SteamFriends.GetFriendRichPresence(userId, SteamRichPresences.RATING_ICON).TryParseToInt();
        }*/

        internal static async Task<List<Tuple<LeaderboardEntry_t, int[]>>> GetLeaderboardEntries(string leaderboardName, ELeaderboardDataRequest type, int start, int end, 
            int timeout = 10000,
            Func<List<Tuple<LeaderboardEntry_t, int[]>>, bool> resultHandler = null,
            Func<bool> repeatHandler = null)
        {
            var resultLeaderboard = await GetLeaderboard(leaderboardName);

            return await TaskHelper.RepeatTaskForeverIfFailed(() => SteamApiHelper.HandleApiCall<List<Tuple<LeaderboardEntry_t, int[]>>, LeaderboardScoresDownloaded_t>(SteamUserStats.DownloadLeaderboardEntries(resultLeaderboard.m_hSteamLeaderboard, type, start, end), CancellationToken.None,
                (tcs, result, bIOFailure) =>
                {
                    if (result.m_hSteamLeaderboard.m_SteamLeaderboard == 0 || bIOFailure)
                    {
                        tcs.SetException(new Exception("Ошибка GetLeaderboardEntries"));
                        return;
                    }

                    var entriesList = new List<Tuple<LeaderboardEntry_t, int[]>>();
                    LeaderboardEntry_t entry;

                    for (int index = 0; index < result.m_cEntryCount; index++)
                    {
                        int[] pData = new int[64];

                        if (SteamUserStats.GetDownloadedLeaderboardEntry(result.m_hSteamLeaderboardEntries, index, out entry, pData, 64))
                        {
                            entriesList.Add(Tuple.Create(entry, pData));
                        }
                    }

                    tcs.SetResult(entriesList);
                }), timeout, CancellationToken.None, resultHandler: resultHandler, repeatHandler: repeatHandler);
        }

        internal static async Task<List<Tuple<LeaderboardEntry_t, int[]>>> GetLeaderboardEntriesForUsers(string leaderboardName, CSteamID[] steamIds,
            int timeout = 10000,
            Func<List<Tuple<LeaderboardEntry_t, int[]>>, bool> resultHandler = null,
            Func<bool> repeatHandler = null)
        {
            var resultLeaderboard = await GetLeaderboard(leaderboardName);

            return await TaskHelper.RepeatTaskForeverIfFailed(() => SteamApiHelper.HandleApiCall<List<Tuple<LeaderboardEntry_t, int[]>>, LeaderboardScoresDownloaded_t>(SteamUserStats.DownloadLeaderboardEntriesForUsers(resultLeaderboard.m_hSteamLeaderboard, steamIds, steamIds.Length), CancellationToken.None,
                (tcs, result, bIOFailure) =>
                {
                    if (result.m_hSteamLeaderboard.m_SteamLeaderboard == 0 || bIOFailure)
                    {
                        tcs.SetException(new Exception("Ошибка GetLeaderboardEntriesForUsers"));
                        return;
                    }

                    var entriesList = new List<Tuple<LeaderboardEntry_t, int[]>>();
                    LeaderboardEntry_t entry;

                    for (int index = 0; index < result.m_cEntryCount; index++)
                    {
                        int[] pData = new int[64];

                        if (SteamUserStats.GetDownloadedLeaderboardEntry(result.m_hSteamLeaderboardEntries, index, out entry, pData, 64))
                            entriesList.Add(Tuple.Create(entry, pData));
                    }

                    tcs.SetResult(entriesList);
                }), timeout, CancellationToken.None, resultHandler: resultHandler, repeatHandler: repeatHandler);
        }

        internal static async Task<LeaderboardEntry_t> GetLeaderboardEntry(string leaderboardName, 
            int timeout = 10000,
            Func<LeaderboardEntry_t, bool> resultHandler = null,
            Func<bool> repeatHandler = null)
        {
            var resultLeaderboard = await GetLeaderboard(leaderboardName);

            return await TaskHelper.RepeatTaskForeverIfFailed(() => SteamApiHelper.HandleApiCall<LeaderboardEntry_t, LeaderboardScoresDownloaded_t>(SteamUserStats.DownloadLeaderboardEntriesForUsers(resultLeaderboard.m_hSteamLeaderboard, new CSteamID[] { SteamUser.GetSteamID() }, 1), CancellationToken.None,
                (tcs, result, bIOFailure) =>
                {
                    if (result.m_hSteamLeaderboard.m_SteamLeaderboard == 0 || bIOFailure)
                    {
                        tcs.SetException(new Exception("Ошибка GetLeaderboardEntry"));
                        return;
                    }

                    var entry = new LeaderboardEntry_t();

                    for (int index = 0; index < result.m_cEntryCount; index++)
                    {
                        SteamUserStats.GetDownloadedLeaderboardEntry(result.m_hSteamLeaderboardEntries, index, out entry, null, 0);
                    }

                    tcs.SetResult(entry);
                }), timeout, CancellationToken.None, resultHandler: resultHandler, repeatHandler: repeatHandler);
        }

        internal static async Task<Tuple<LeaderboardEntry_t, int[]>> GetLeaderboardEntryWithDetails(string leaderboardName, 
            int timeout = 10000,
            Func<Tuple<LeaderboardEntry_t, int[]>, bool> resultHandler = null,
            Func<bool> repeatHandler = null)
        {
            var resultLeaderboard = await GetLeaderboard(leaderboardName);

            return await TaskHelper.RepeatTaskForeverIfFailed(() => SteamApiHelper.HandleApiCall<Tuple<LeaderboardEntry_t, int[]>, LeaderboardScoresDownloaded_t>(SteamUserStats.DownloadLeaderboardEntriesForUsers(resultLeaderboard.m_hSteamLeaderboard, new CSteamID[] { SteamUser.GetSteamID() }, 1), CancellationToken.None,
                (tcs, result, bIOFailure) =>
                {
                    if (result.m_hSteamLeaderboard.m_SteamLeaderboard == 0 || bIOFailure)
                    {
                        tcs.SetException(new Exception("Ошибка GetLeaderboardEntryWithDetails"));
                        return;
                    }

                    int[] pData = null;

                    if (result.m_cEntryCount > 0)
                        pData = new int[64];

                    var entry = new LeaderboardEntry_t();

                    for (int index = 0; index < result.m_cEntryCount; index++)
                    {
                        SteamUserStats.GetDownloadedLeaderboardEntry(result.m_hSteamLeaderboardEntries, index, out entry, pData, 64);
                    }

                    tcs.SetResult(Tuple.Create(entry, pData));
                }), timeout, CancellationToken.None, resultHandler: resultHandler, repeatHandler: repeatHandler);
        }

        internal static async Task<bool> SetLeaderboardValue(string leaderboardName, int value, int[] pDataValue, 
            int timeout = 10000,
            Func<bool, bool> resultHandler = null,
            Func<bool> repeatHandler = null)
        {
            if (pDataValue?.Length > 64)
                return false;

            var resultLeaderboard = await GetLeaderboard(leaderboardName);

            return await TaskHelper.RepeatTaskForeverIfFailed(() => SteamApiHelper.HandleApiCall<bool, LeaderboardScoreUploaded_t>(SteamUserStats.UploadLeaderboardScore(resultLeaderboard.m_hSteamLeaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, value, pDataValue, pDataValue == null ? 0 : pDataValue.Length), CancellationToken.None,
                (tcs, result, bIOFailure) =>
                {
                    if (result.m_bSuccess == 0 || bIOFailure)
                    {
                        tcs.SetException(new Exception("Ошибка запроса OnUploadLeaderboardScore"));
                        return;
                    }

                    if (result.m_bScoreChanged == 1)
                    {
                        tcs.SetResult(true);
                        return;
                    }

                    tcs.SetException(new Exception("Ошибка запроса OnUploadLeaderboardScore"));
                }), timeout, CancellationToken.None, resultHandler: resultHandler, repeatHandler: repeatHandler);
        }

        internal static async Task<LeaderboardFindResult_t> GetLeaderboard(string leaderboardName, 
            int timeout = 10000,
            Func<LeaderboardFindResult_t, bool> resultHandler = null,
            Func<bool> repeatHandler = null)
        {
            return await TaskHelper.RepeatTaskForeverIfFailed(() => SteamApiHelper.HandleApiCall<LeaderboardFindResult_t>(SteamUserStats.FindOrCreateLeaderboard(leaderboardName, ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending, ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric), CancellationToken.None,
                (tcs, result, bIOFailure) =>
                {
                    if (result.m_bLeaderboardFound == 0 || bIOFailure)
                    {
                        tcs.SetException(new Exception("Ошибка запроса GetLeaderboard"));
                        return;
                    }

                    tcs.SetResult(result);
                }), timeout, CancellationToken.None, resultHandler: resultHandler, repeatHandler: repeatHandler);
        }

        internal static void CancelApiCall<T>()
        {
            CallResultHolder<T>.Cancel();
        }

        internal static void HandleApiCall<T>(SteamAPICall_t call, CancellationToken token, Action<T, bool> handler)
        {
            if (token.IsCancellationRequested)
                return;

            CallResultHolder<T>.Set(call, (result, bIOFailure) => handler(result, bIOFailure));
        }

        internal static void HandleApiCall<T, B>(SteamAPICall_t call, CancellationToken token, Action<B, bool> handler)
        {
            if (token.IsCancellationRequested)
                return;

            CallResultHolder<B>.Set(call, (result, bIOFailure) => handler(result, bIOFailure));
        }

        internal static Task<T> HandleApiCall<T>(SteamAPICall_t call, CancellationToken token, Action<TaskCompletionSource<T>, T, bool> handler)
        {
            return TaskHelper.FromAction<T>(tsc =>
            {
                if (token.IsCancellationRequested)
                {
                    tsc.TrySetCanceled();
                    return;
                }

                CallResultHolder<T>.Set(call, (result, bIOFailure) => handler(tsc, result, bIOFailure));
                token.Register(() => tsc.TrySetCanceled());

            });
        }

        internal static Task<T> HandleApiCall<T, B>(SteamAPICall_t call, CancellationToken token, Action<TaskCompletionSource<T>, B, bool> handler)
        {
            return TaskHelper.FromAction<T>(tsc =>
            {
                if (token.IsCancellationRequested)
                {
                    tsc.TrySetCanceled();
                    return;
                }

                CallResultHolder<B>.Set(call, (result, bIOFailure) => handler(tsc, result, bIOFailure));
                token.Register(() => tsc.TrySetCanceled());
            });
        }

        private static class CallResultHolder<T>
        {
            private static readonly CallResult<T> _callback = CallResult<T>.Create();
            
            internal static void Cancel()
            {
                _callback.Cancel();
            }

            internal static void Set(SteamAPICall_t hAPICall, CallResult<T>.APIDispatchDelegate func)
            {
                Cancel();
                _callback.Set(hAPICall, func);
            }
        }
    }
}