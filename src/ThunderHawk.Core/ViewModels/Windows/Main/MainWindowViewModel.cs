using Framework;
using System.Collections.ObjectModel;

namespace ThunderHawk.Core
{
    public class MainWindowViewModel : WindowViewModel
    {
        public ActionFrame GoBack { get; } = new ActionFrame();
        public ActionFrame GoForward { get; } = new ActionFrame();

        public ListFrame<EmbeddedPageViewModel> Pages { get; } = new ListFrame<EmbeddedPageViewModel>();

        public NavigationPanelFrame NavigationPanel { get; } = new NavigationPanelFrame();

        public ActionFrame OpenSettings { get; } = new ActionFrame();

        public MainWindowViewModel()
        {
            Pages.DataSource = new ObservableCollection<EmbeddedPageViewModel>()
            {
                new MainPageViewModel(),
                new ChatPageViewModel(),
                //new ModificationsPageViewModel(){ Enabled = false },
                //new AllNewsPageViewModel(){ Enabled = false }
            };

            NavigationPanel.CurrentContentViewModel = Pages.SelectedItem = Pages.DataSource[0];
        }
    }
}
