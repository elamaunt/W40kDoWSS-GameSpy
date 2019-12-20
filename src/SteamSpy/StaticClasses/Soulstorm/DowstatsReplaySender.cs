using System;
using System.Threading;
using SharedServices;
using System.Web;
using Logger = ThunderHawk.Core.Logger;

namespace ThunderHawk.StaticClasses.Soulstorm
{
    public static class DowstatsReplaySender
    {
        
        private static readonly string DowstatsUploadReplayUrl;
        
        static DowstatsReplaySender()
        {
            DowstatsUploadReplayUrl = "http://" +
                                      (Environment.GetEnvironmentVariable("dowstatsServer") ?? "dowstats.ru") +
                                      "/thunderhawkConnect/replayReceive.php?";
        }

        public static void SendReplay(GameFinishedMessage gfm)
        {
            Thread dowstatsSendThread = new Thread(SendReplayAsync);
            dowstatsSendThread.Start(gfm); 
        }
        private static void SendReplayAsync(object gameFinishedMessage)
        {
            try
            {
                // waiting 10 second, untill main server register game in dowstats
                Thread.Sleep(10000);
                
                var updateRequestBuilder = new UriBuilder(DowstatsUploadReplayUrl);

                var gfg = (GameFinishedMessage) gameFinishedMessage;

                var parameters = HttpUtility.ParseQueryString(string.Empty);// url params storage

                for (int index = 0; index < gfg.Players.Length; index++)
                {
                    parameters["p" + (index+1)] = gfg.Players[index].Name;
                }
                
                updateRequestBuilder.Query = parameters.ToString();
                
                System.Net.WebClient webClient = new System.Net.WebClient();
                webClient.Headers.Add("Content-Type", "binary/octet-stream");

                byte[] result = webClient.UploadFile(updateRequestBuilder.Uri.ToString(),
                    "POST",
                    $"{PathFinder.GamePath}\\Playback\\temp.rec");

                Logger.Info("Successful push replay to dowstats. Callback: " + System.Text.Encoding.UTF8.GetString(result));
            }
            catch (Exception e)
            {
                Logger.Error("Error occurred while sending replay to dowstats dowstats");
                Logger.Debug(e.StackTrace);
            }
            
        }
        
    }
}
