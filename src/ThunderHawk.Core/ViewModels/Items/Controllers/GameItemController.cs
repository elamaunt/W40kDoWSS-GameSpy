using Framework;
using System;
using System.IO;
using System.Net.Http;
using System.Web;

namespace ThunderHawk.Core
{
    public class GameItemController : FrameController<GameItemViewModel>
    {
        static HttpClient _client = new HttpClient();
        string FilePath => Path.Combine(CoreContext.LaunchService.GamePath, "Playback", Frame.Game.SessionId
            .Replace('<', '#')
            .Replace(">", string.Empty)
            .Replace("{", string.Empty)
            .Replace("}", string.Empty)
            .Replace("|", string.Empty)
            .Replace("[", string.Empty)
            .Replace("]", string.Empty)
            .Replace("/", string.Empty)
            .Replace("\\", string.Empty)
            + ".rec");

        protected override void OnBind()
        {
            CoreContext.MasterServer.NewGameReceived += OnNewGameReceived;

            ChechReplayInPlayback();

            Frame.Download.Action = () =>
            {
                var url = new Uri(@"http://dowstats.ru/thunderhawk/replayByServerId.php?id=" + HttpUtility.UrlEncode(Frame.Game.SessionId));

                _client.GetAsync(url, HttpCompletionOption.ResponseContentRead).OnContinueOnUi(task =>
                {
                    if (task.IsFaulted)
                    {
                        CoreContext.SystemService.NotifyAsWindowToastMessage("Failed to download replay");
                    }
                    else
                    {
                        try
                        {
                            task.Result.Content.ReadAsByteArrayAsync().OnContinueOnUi(readTask =>
                            {
                                if (task.IsFaulted)
                                {
                                    CoreContext.SystemService.NotifyAsWindowToastMessage("Failed to download replay");
                                }
                                else
                                {
                                    try
                                    {
                                        var filePath = FilePath;
                                        File.WriteAllBytes(filePath, readTask.Result);
                                        ChechReplayInPlayback();
                                    }
                                    catch (Exception)
                                    {
                                        CoreContext.SystemService.NotifyAsWindowToastMessage("Failed to download replay");
                                    }
                                }
                            });
                        }
                        catch (Exception)
                        {
                            CoreContext.SystemService.NotifyAsWindowToastMessage("Failed to download replay");
                        }
                    }
                });
            };

        }

        void ChechReplayInPlayback()
        {
            var filePath = FilePath;

            var check = File.Exists(filePath);

            Frame.Downloaded.Visible = check;
            Frame.Download.Visible = !check;
        }

        private void OnNewGameReceived(GameInfo obj)
        {
            RunOnUIThread(() =>
            { 
                Frame.Date.Text = Frame.ToDateValue(Frame.Game.PlayedDate);
            });
        }

        protected override void OnUnbind()
        {
            CoreContext.MasterServer.NewGameReceived -= OnNewGameReceived;
            base.OnUnbind();
        }
    }
}
