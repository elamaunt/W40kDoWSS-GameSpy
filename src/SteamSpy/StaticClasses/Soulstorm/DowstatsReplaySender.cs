using SharedServices;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
                                      "/thunderhawk/replayReceive.php?";
        }

        public static void SendReplay(GameFinishedMessage message)
        {
            Task.Delay(10000).ContinueWith(t =>
            {
                try
                {
                    var updateRequestBuilder = new UriBuilder(DowstatsUploadReplayUrl);

                    var parameters = HttpUtility.ParseQueryString(string.Empty);// url params storage

                    for (int index = 0; index < message.Players.Length; index++)
                    {
                        parameters["p" + (index + 1)] = message.Players[index].Name;
                    }

                    updateRequestBuilder.Query = parameters.ToString();

                    System.Net.WebClient webClient = new System.Net.WebClient();
                    webClient.Headers.Add("Content-Type", "binary/octet-stream");

                    var filePath = Path.Combine(PathFinder.GamePath, "Playback", "temp.rec");

                    if (!File.Exists(filePath))
                        return;

                    var result = webClient.UploadFile(updateRequestBuilder.Uri.ToString(), "POST", filePath);

                    Logger.Info("Successful push replay to dowstats. Callback: " + System.Text.Encoding.UTF8.GetString(result));
                }
                catch (Exception e)
                {
                    Logger.Error("Error occurred while sending replay to dowstats dowstats");
                    Logger.Debug(e.StackTrace);
                }
            });
        }
        
    }
}
