using System;
using System.Collections.ObjectModel;
using System.Linq;
using Framework;

namespace ThunderHawk.Core
{
    public class MainPageController : FrameController<MainPageViewModel>
    {
        protected override void OnBind()
        {
            // Logger.Log("Test log");

            Frame.Title.Text = CoreContext.LangService.GetString("MainPage");

            Frame.LaunchGame.Action = LaunchGame;

            Frame.News.DataSource = CoreContext.NewsProvider.GetNews()
                .Select(x => new NewsItemViewModel(x))
                .ToObservableCollection();
        }

        void LaunchGame()
        {
            // TODO: launch SS1.2
        }
    }
}
