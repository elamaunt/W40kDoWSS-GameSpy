using Framework;
using System.Collections.ObjectModel;

namespace ThunderHawk.Core
{
    public class MainWindowViewModel : WindowViewModel
    {
        public ListFrame<EmbeddedPageViewModel> Pages { get; } = new ListFrame<EmbeddedPageViewModel>();

        public MainWindowViewModel()
        {
            Pages.DataSource = new ObservableCollection<EmbeddedPageViewModel>()
            {
                new MainPageViewModel(),
                new ChatPageViewModel(),
                //new ModificationsPageViewModel(){ Enabled = false },
                //new AllNewsPageViewModel(){ Enabled = false }
            };

            Pages.SelectedItem = Pages.DataSource[0];
        }
    }
}
