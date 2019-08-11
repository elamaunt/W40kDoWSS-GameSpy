using Framework;
using System.Linq;

namespace ThunderHawk.Core
{
    public class MainPageController : BindingController<IMainPage, MainPageViewModel>
    {
        protected override void OnBind()
        {
            // Logger.Log("Test log");

            Frame.Title.Text = CoreContext.LangService.GetString("MainPage");

            Frame.LaunchGame.Action = LaunchGame;

            var newsSource = CoreContext.NewsProvider.GetNews()
                .Select(x => new NewsItemViewModel(x))
                .ToObservableCollection();

            newsSource[0].Big = true;
            Frame.News.DataSource = newsSource;
        }

        void LaunchGame()
        {

            View.OpenWindow(new SettingsWindowViewModel());
            // TODO: launch SS1.2
        }
    }
}
