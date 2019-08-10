using Framework;
using System.Collections.ObjectModel;

namespace ThunderHawk.Core
{
    public class MainWindowViewModel : WindowViewModel
    {
        public ListFrame<PageViewModel> Pages { get; } = new ListFrame<PageViewModel>();

        public MainWindowViewModel()
        {
            Pages.DataSource = new ObservableCollection<PageViewModel>()
            {
                new MainPageViewModel(),
                new ChatPageViewModel(),
                new ModificationsPageViewModel(){ Enabled = false },
                new AllNewsPageViewModel(){ Enabled = false }
            };
        }
    }
}
