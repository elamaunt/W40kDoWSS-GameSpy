using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using GSMasterServer.Data;
using GSMasterServer.Servers;
using GSMasterServer.Services;
using NLog.Fluent;

namespace GSMasterServer.Utils
{
    
    public class Dowstats
    {
        private static readonly string DowstatsConnectUrl;
        private static readonly string DowstatsVersion = Environment.GetEnvironmentVariable("dowstatsVersion") ?? "108";
        
        static Dowstats()
        {
            DowstatsConnectUrl = "http://" + 
                                 (Environment.GetEnvironmentVariable("dowstatsServer") ?? "dowstats.ru") +
                                 "/elmauntConnect.php?";
        }

        
        public static void UploadGame(Dictionary<string, string> gameInfo, GameUserInfo[] gameUserInfo, bool isRateGame)
        {

            var updateRequestBuilder = new UriBuilder(DowstatsConnectUrl);
            var parameters = HttpUtility.ParseQueryString(string.Empty);// url params storage
            
            var playerCount = int.Parse(gameInfo["Players"]);

            var winCounter = 0;

            for (int i = 0; i < gameUserInfo.Length; i++)
            {
                var dowStatsPIndex = i + 1;
                parameters["p" + dowStatsPIndex] = gameInfo["player_" + i];
                parameters["id" + dowStatsPIndex] = gameInfo["PID_" + i];
                parameters["mmr1x1p" + dowStatsPIndex] = gameUserInfo[i].Profile.Score1v1.ToString();
                parameters["mmr2x2p" + dowStatsPIndex] = gameUserInfo[i].Profile.Score2v2.ToString();
                parameters["mmr3x3p" + dowStatsPIndex] = gameUserInfo[i].Profile.Score3v3.ToString();
                switch (gameUserInfo[i].Race)
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

                if (gameUserInfo[i].FinalState == PlayerFinalState.Winner)
                {
                    winCounter++;
                    parameters["w" + winCounter] = (i + 1).ToString();
                }
            }
            
            if(playerCount % 2 != 0 && playerCount%2 != winCounter) return; // ФФА не нужно
            
            parameters["type"] = winCounter.ToString();
            parameters["map"] = gameInfo["Scenario"];
            parameters["mod"] = "el" + gameInfo["Mod"];
            parameters["gtime"] = gameInfo["Duration"];
            parameters["winby"] = gameInfo["WinBy"];
            parameters["isRate"] = isRateGame.ToString();
            parameters["version"] = DowstatsVersion;
            
            updateRequestBuilder.Query = parameters.ToString();

            var request = (HttpWebRequest)WebRequest.Create( updateRequestBuilder.Uri );
            request.Method = "get";
 
            Logger.Info("Start request to" + DowstatsConnectUrl);
            Logger.Debug($"request uri: {updateRequestBuilder.Uri}");
            request.BeginGetResponse( OnAsyncCallback, request );
            
        }
        
        private static void OnAsyncCallback( IAsyncResult asyncResult ) {
            var httpWebRequest = (HttpWebRequest)asyncResult.AsyncState;
            WebResponse response = httpWebRequest.EndGetResponse( asyncResult );
            var reader = new StreamReader( response.GetResponseStream() );
            string str = reader.ReadToEnd();
            Logger.Info( "Response from dowstats: " + str );
        }
    }
}