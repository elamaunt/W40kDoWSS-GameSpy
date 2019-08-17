using Framework;
using System.Linq;

namespace ThunderHawk.Core
{
    public class MainPageController : FrameController<MainPageViewModel>
    {
        protected override void OnBind()
        {
            Frame.Title.Text = CoreContext.LangService.GetString("MainPage");

            Frame.LaunchGame.Action = LaunchGame;

            Frame.ErrorsFound.Enabled = false;

            CoreContext.NewsProvider.GetNews()
                .OnCompletedOnUi(news =>
                {
                    var newsSource = news.Select(x =>
                    {
                        var itemVM = new NewsItemViewModel(x);

                        itemVM.Navigate.Action = () => Frame.GlobalNavigationManager?.OpenPage<NewsViewerPageViewModel>(bundle =>
                        {
                            bundle.SetString(nameof(NewsViewerPageViewModel.NewsItem), x.AsJson());
                        });

                        return itemVM;
                    })
                    .ToObservableCollection();

                    newsSource[0].Big = true;
                    Frame.News.DataSource = newsSource;
                });
        }

        void LaunchGame()
        {
            CoreContext.LaunchService.LaunchGame();
        }
    }
}
