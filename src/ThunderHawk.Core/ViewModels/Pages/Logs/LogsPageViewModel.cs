using Framework;

namespace ThunderHawk.Core
{
    public class LogsPageViewModel : EmbeddedPageViewModel
    {
        public ListFrame<LogMessageItemViewModel> Messages { get; } = new ListFrame<LogMessageItemViewModel>();

        public LogsPageViewModel()
        {
            TitleButton.Text = "LOGS";
        }
    }
}
