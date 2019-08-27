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
        public TextFrame Version { get; } = new TextFrame() { Text = "BETA 1.0" };
        
        public MainWindowViewModel()
        {
            Pages.DataSource = new ObservableCollection<PageTabViewModel>()
            {
                new PageTabViewModel(new MainPageViewModel()),
                new PageTabViewModel(new TweaksPageViewModel()),
                new PageTabViewModel(new FAQPageViewModel())
                
                //new ModificationsPageViewModel(){ Enabled = false },
                //new AllNewsPageViewModel(){ Enabled = false }
            };

            Pages.SelectedItem = Pages.DataSource[0];
            NavigationPanel.CurrentContentViewModel = Pages.DataSource[0].ViewModel;
        }
    }
}
