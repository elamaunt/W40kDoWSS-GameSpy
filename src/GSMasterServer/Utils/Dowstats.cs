using GSMasterServer.Data;
using IrcNet.Tools;
using SharedServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;

namespace GSMasterServer.Utils
{

    public class Dowstats
    {
        static readonly HttpClient httpClient = new HttpClient();
        
        private static readonly string DowstatsUploadGameUrl;
        private static readonly string DowstatsUploadPlayerStatsUrl;
        private static readonly string DowstatsVersion = Environment.GetEnvironmentVariable("dowstatsVersion") ?? "108";
        
        private static readonly int DefaultTimeout = 20 * 1000;// 20 second
        
        static Dowstats()
        {
            DowstatsUploadGameUrl = "http://" + 
                                 (Environment.GetEnvironmentVariable("dowstatsServer") ?? "dowstats.ru") +
                                 "/thunderhawkConnect/connect.php?";
            
            DowstatsUploadPlayerStatsUrl = "http://" + 
                                    (Environment.GetEnvironmentVariable("dowstatsServer") ?? "dowstats.ru") +
                                    "/thunderhawkConnect/updatePlayer.php?";
            
        }

        
        public static void UploadGame(GamePlayerInfo[] gameUserInfo, GameDBO game)
        {

            var updateRequestBuilder = new UriBuilder(DowstatsUploadGameUrl);
            var parameters = HttpUtility.ParseQueryString(string.Empty);// url params storage
            
            var playerCount = gameUserInfo.Length;

            var winCounter = 0;

            for (int i = 0; i < gameUserInfo.Length; i++)
            {
                var dowStatsPIndex = i + 1;
                parameters["p" + dowStatsPIndex] = gameUserInfo[i].Profile.Name;
                parameters["id" + dowStatsPIndex] = gameUserInfo[i].Profile.Id.ToString();
                parameters["mmr1x1p" + dowStatsPIndex] = gameUserInfo[i].Profile.Score1v1.ToString();
                parameters["mmr2x2p" + dowStatsPIndex] = gameUserInfo[i].Profile.Score2v2.ToString();
                parameters["mmr3x3p" + dowStatsPIndex] = gameUserInfo[i].Profile.Score3v3.ToString();
                switch (gameUserInfo[i].Part.Race)
                {
                    case Race.space_marine_race:
                        parameters["r" + dowStatsPIndex] = "1";
                        break;
                    case Race.chaos_marine_race:
                        parameters["r" + dowStatsPIndex] = "2";
                        break;
                    case Race.ork_race:
                        parameters["r" + dowStatsPIndex] = "3";
                        break;
                    case Race.eldar_race:
                        parameters["r" + dowStatsPIndex] = "4";
                        break;
                    case Race.guard_race:
                        parameters["r" + dowStatsPIndex] = "5";
                        break;
                    case Race.necron_race:
                        parameters["r" + dowStatsPIndex] = "6";
                        break;
                    case Race.tau_race:
                        parameters["r" + dowStatsPIndex] = "7";
                        break;
                    case Race.dark_eldar_race:
                        parameters["r" + dowStatsPIndex] = "9"; // не опечатка, на сервере статистики ДЕ это номер 9
                        break;
                    case Race.sisters_race:
                        parameters["r" + dowStatsPIndex] = "8";
                        break;
                }
                
                if (gameUserInfo[i].Part.FinalState == PlayerFinalState.Winner)
                {
                    winCounter++;
                    parameters["w" + winCounter] = (i + 1).ToString();
                }
            }
            
            if(playerCount % 2 != 0 && playerCount%2 != winCounter) return; // ФФА не нужно


            var gameType = winCounter.ToString();    
            
            parameters["type"] = gameType;
            parameters["map"] = game.Map;
            parameters["mod"] = "el" + game.ModName;
            parameters["modVersion"] = game.ModVersion;
            parameters["gtime"] = game.Duration.ToString();
            parameters["isRate"] = game.IsRateGame.ToString();
            parameters["thunderhawkId"] = game.Id;
            parameters["version"] = DowstatsVersion;
            
            updateRequestBuilder.Query = parameters.ToString();

            var request = (HttpWebRequest)WebRequest.Create( updateRequestBuilder.Uri );
            request.Method = "get";
 
            Logger.Trace("[UploadGame]Start request to " + DowstatsUploadGameUrl);
            Logger.Trace($"request uri: {updateRequestBuilder.Uri}");
            IAsyncResult result = request.BeginGetResponse( OnAsyncCallback, request );
            
            for (int i = 0; i < gameUserInfo.Length; i++)
            {
                UpdateDetailStats(
                    gameUserInfo[i].Profile.Id, 
                    Int32.Parse(gameType), 
                    gameUserInfo[i].Part.Race, 
                    gameUserInfo[i].Part.FinalState == PlayerFinalState.Winner,
                    game.IsRateGame);
            }
            
        }

        private static void UpdateDetailStats(long profileId, int gameType, Race race, bool isWin, bool isAuto)
        {
            switch (gameType)
            {
                case 1:
                    var profile1X1 = Database.MainDBInstance.GetProfile1X1ByProfileId(profileId);
                    if (profile1X1 == null)
                    {
                        profile1X1 = Database.MainDBInstance.CreateProfile1X1(profileId);
                    }
                    var profile1X1ToUpdate = (Profile1X1DBO) profile1X1.UpdateRaceStat(race, isWin, isAuto);
                    Database.MainDBInstance.UpdateProfile1X1(profile1X1ToUpdate);
                    UploadProfileToDowstats(profile1X1ToUpdate, gameType);
                    break;
                case 2:
                    var profile2X2 = Database.MainDBInstance.GetProfile2X2ByProfileId(profileId);
                    if (profile2X2 == null)
                    {
                        profile2X2 = Database.MainDBInstance.CreateProfile2X2(profileId);
                    }
                    var profile2X2ToUpdate = (Profile2X2DBO) profile2X2.UpdateRaceStat(race, isWin, isAuto);
                    Database.MainDBInstance.UpdateProfile2X2(profile2X2ToUpdate);
                    UploadProfileToDowstats(profile2X2ToUpdate, gameType);
                    break;
                case 3:
                    var profile3X3 = Database.MainDBInstance.GetProfile3X3ByProfileId(profileId);
                    if (profile3X3 == null)
                    {
                        profile3X3 = Database.MainDBInstance.CreateProfile3X3(profileId);
                    }
                    var profile3X3ToUpdate = (Profile3X3DBO) profile3X3.UpdateRaceStat(race, isWin, isAuto);
                    Database.MainDBInstance.UpdateProfile3X3(profile3X3ToUpdate);
                    UploadProfileToDowstats(profile3X3ToUpdate, gameType);
                    break;
                case 4:
                    var profile4X4 = Database.MainDBInstance.GetProfile4X4ByProfileId(profileId);
                    if (profile4X4 == null)
                    {
                        profile4X4 = Database.MainDBInstance.CreateProfile4X4(profileId);
                    }
                    var profile4X4ToUpdate = (Profile4X4DBO) profile4X4.UpdateRaceStat(race, isWin, isAuto);
                    Database.MainDBInstance.UpdateProfile4X4(profile4X4ToUpdate);
                    UploadProfileToDowstats(profile4X4ToUpdate, gameType);
                    break;
            }
        }

        private static void UploadProfileToDowstats(ProfileDetailDBO profile, int gameType)
        {
            var updateRequestBuilder = new UriBuilder(DowstatsUploadPlayerStatsUrl);
            var parameters = HttpUtility.ParseQueryString(string.Empty);// url params storage
            
            parameters["type"] = gameType.ToString();
            parameters["serverId"] = profile.ProfileId.ToString();
            parameters["SmGamesCount"] = profile.SmGamesCount.ToString();
            parameters["CsmGamesCount"] = profile.CsmGamesCount.ToString();
            parameters["OrkGamesCount"] = profile.OrkGamesCount.ToString();
            parameters["EldarGamesCount"] = profile.EldarGamesCount.ToString();
            parameters["IgGamesCount"] = profile.IgGamesCount.ToString();
            parameters["NecGamesCount"] = profile.NecGamesCount.ToString();
            parameters["TauGamesCount"] = profile.TauGamesCount.ToString();
            parameters["DeGamesCount"] = profile.DeGamesCount.ToString();
            parameters["SobGamesCount"] = profile.SobGamesCount.ToString();
            
            parameters["SmWinCount"] = profile.SmWinCount.ToString();
            parameters["CsmWinCount"] = profile.CsmWinCount.ToString();
            parameters["OrkWinCount"] = profile.OrkWinCount.ToString();
            parameters["EldarWinCount"] = profile.EldarWinCount.ToString();
            parameters["IgWinCount"] = profile.IgWinCount.ToString();
            parameters["NecrWinCount"] = profile.NecrWinCount.ToString();
            parameters["TauWinCount"] = profile.TauWinCount.ToString();
            parameters["DeWinCount"] = profile.DeWinCount.ToString();
            parameters["SobWinCount"] = profile.SobWinCount.ToString();
            
            parameters["SmGamesCountAuto"] = profile.SmGamesCountAuto.ToString();
            parameters["CsmGamesCountAuto"] = profile.CsmGamesCountAuto.ToString();
            parameters["OrkGamesCountAuto"] = profile.OrkGamesCountAuto.ToString();
            parameters["EldarGamesCountAuto"] = profile.EldarGamesCountAuto.ToString();
            parameters["IgGamesCountAuto"] = profile.IgGamesCountAuto.ToString();
            parameters["NecrGamesCountAuto"] = profile.NecrGamesCountAuto.ToString();
            parameters["TauGamesCountAuto"] = profile.TauGamesCountAuto.ToString();
            parameters["DeGamesCountAuto"] = profile.DeGamesCountAuto.ToString();
            parameters["SobGamesCountAuto"] = profile.SobGamesCountAuto.ToString();
            
            parameters["SmWinCountAuto"] = profile.SmWinCountAuto.ToString();
            parameters["CsmWinCountAuto"] = profile.CsmWinCountAuto.ToString();
            parameters["OrkWinCountAuto"] = profile.OrkWinCountAuto.ToString();
            parameters["EldarWinCountAuto"] = profile.EldarWinCountAuto.ToString();
            parameters["IgWinCountAuto"] = profile.IgWinCountAuto.ToString();
            parameters["NecrWinCountAuto"] = profile.NecrWinCountAuto.ToString();
            parameters["TauWinCountAuto"] = profile.TauWinCountAuto.ToString();
            parameters["DeWinCountAuto"] = profile.DeWinCountAuto.ToString();
            parameters["SobWinCountAuto"] = profile.SobWinCountAuto.ToString();
            
            updateRequestBuilder.Query = parameters.ToString();

            var request = (HttpWebRequest)WebRequest.Create( updateRequestBuilder.Uri );
            request.Method = "get";
 
            Logger.Trace("Start request to " + DowstatsUploadPlayerStatsUrl);
            Logger.Trace($"request uri: {updateRequestBuilder.Uri}");
            IAsyncResult result = request.BeginGetResponse( OnAsyncCallback, request );
        }


        private static void OnAsyncCallback(IAsyncResult asyncResult)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)asyncResult.AsyncState;
                WebResponse response = httpWebRequest.EndGetResponse(asyncResult);
                var reader = new StreamReader(response.GetResponseStream());
                string str = reader.ReadToEnd();
                Logger.Trace("Response from dowstats: " + str);
                reader.Close();
            }
            catch (Exception e)
            {
                Logger.Warn(e);
            }
        }
    }
}