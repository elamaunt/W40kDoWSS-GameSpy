using Framework;

namespace ThunderHawk.Core
{
    public class MainWindowTabsController : FrameController<MainWindowViewModel>
    {
        protected override void OnBind()
        {
            foreach (var item in Frame.Pages.DataSource)
                item.TitleButton.Action = () => OnTabClicked(item);
        }

        void OnTabClicked(EmbeddedPageViewModel model)
        {
            Frame.Pages.SelectedItem = model;
        }
    }
}
