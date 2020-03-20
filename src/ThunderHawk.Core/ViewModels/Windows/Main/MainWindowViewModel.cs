using Framework;
using System.Collections.ObjectModel;

namespace ThunderHawk.Core
{
    public class MainWindowViewModel : WindowViewModel
    {
        public ActionFrame GoBack { get; } = new ActionFrame();
        public ActionFrame GoForward { get; } = new ActionFrame();

        public ListFrame<PageTabViewModel> Pages { get; } = new ListFrame<PageTabViewModel>();

        public NavigationPanelFrame NavigationPanel { get; } = new NavigationPanelFrame();

        public ActionFrame OpenSettings { get; } = new ActionFrame();
        public TextFrame Version { get; } = new TextFrame() { Text = CoreContext.UpdaterService.CurrentVersionUI };

        public TextFrame UserAccount { get; } = new TextFrame();

        public StatsPageViewModel StatsViewModel { get; }
        public ChatPageViewModel ChatViewModel { get; }
        public PageTabViewModel ChatTabViewModel { get; }
        public LogsPageViewModel LogsViewModel { get; }
        
        public MainWindowViewModel()
        {
            Pages.DataSource = new ObservableCollection<PageTabViewModel>()
            {
                new PageTabViewModel(new MainPageViewModel()),
                (ChatTabViewModel = new PageTabViewModel(ChatViewModel = new ChatPageViewModel())),
                new PageTabViewModel(StatsViewModel = new StatsPageViewModel()),
                new PageTabViewModel(new TweaksPageViewModel()),
                new PageTabViewModel(LogsViewModel = new LogsPageViewModel())
            };

            Pages.SelectedItem = Pages.DataSource[0];
            NavigationPanel.CurrentContentViewModel = Pages.DataSource[0].ViewModel;
        }
    }
}
