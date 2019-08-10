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

            Frame.News.DataSource = CoreContext.NewsProvider.GetNews()
                .Select(x => new NewsItemViewModel(x))
                .ToObservableCollection();
        }

        void LaunchGame()
        {

            View.OpenWindow(new SettingsWindowViewModel());
            // TODO: launch SS1.2
        }
    }
}
