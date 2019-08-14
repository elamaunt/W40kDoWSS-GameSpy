using Framework;

namespace ThunderHawk.Core
{
    public class MainWindowTabsController : FrameController<MainWindowViewModel>
    {
        protected override void OnBind()
        {
            foreach (var item in Frame.Pages.DataSource)
                item.TitleButton.Action = () => OnTabClicked(item);

            OnTabClicked(Frame.Pages.SelectedItem);
        }

        void OnTabClicked(EmbeddedPageViewModel model)
        {
            foreach (var item in Frame.Pages.DataSource)
            {
                item.TitleButton.IsChecked = model == item;
                item.TitleButton.Enabled = model != item;
            }

            Frame.Pages.SelectedItem = model;
        }
    }
}
